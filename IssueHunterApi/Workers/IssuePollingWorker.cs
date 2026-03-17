using System.Net.Sockets;
using System.Reflection.Metadata;
using IssueHunter.Configuration;
using IssueHunter.Data;
using IssueHunter.Dtos.GitHub;
using IssueHunter.Models;
using IssueHunter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IssueHunter.Workers;

public class IssuePollingWorker : BackgroundService
{
    private readonly ILogger<IssuePollingWorker> _logger;
    private readonly GitHubService _github;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IOptionsMonitor<IssuePollingOptions> _options; 
    
    public IssuePollingWorker(
        ILogger<IssuePollingWorker> logger, 
        GitHubService github, 
        IDbContextFactory<AppDbContext> dbContextFactory,
        IOptionsMonitor<IssuePollingOptions> options)
    {
        _logger = logger;
        _github = github;
        _dbContextFactory = dbContextFactory;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _options.CurrentValue;
        
        if (!settings.Enabled)
        {
            _logger.LogInformation("Issue polling is disabled via configuration.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        var interval = TimeSpan.FromMinutes(settings.IntervalMinutes);
        if (!settings.RunOnStartup)
        {
            await Task.Delay(interval, stoppingToken);
        };
        
        _logger.LogInformation("Issue polling worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                await PollGitHubAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred when polling GitHub: " + ex.Message);
            }

            settings = _options.CurrentValue;
            interval = TimeSpan.FromMinutes(settings.IntervalMinutes);
            
            await Task.Delay(interval, stoppingToken);
        }
    }
    
    private string ExtractRepoName(string repositoryUrl)
    {
        var parts = repositoryUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            return $"{parts[^2]}/{parts[^1]}";
        }

        return repositoryUrl;
    }

    private async Task PollGitHubAsync(CancellationToken stoppingToken)
    {
        
        
            _logger.LogInformation("Polling GitHub...");

            var now = DateTimeOffset.UtcNow;
            
            await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken);

                var searches = await db.Searches
                    .Where(s => s.Enabled)
                    .OrderByDescending(s => s.Priority)
                    .ToListAsync(stoppingToken);


            searches = searches
                .Where(s => s.NextRunAfter == null || s.NextRunAfter <= now)
                .ToList();

            foreach (var search in searches)
            {
                
                var query = GitHubIssueQueryBuilder.ForSearch(search)
                    .RequireNoAssignee()
                    .SortBy("created")
                    .Build();
                
                var result = await _github.SearchIssuesAsync(query);

                // Only store issues that don't have an active pull request
                var issuesOnly = result.Items.Where(x => x.PullRequest is null).ToList();

                foreach (var ghIssue in issuesOnly)
                {
                    var existingIssue = await db.Issues.FirstOrDefaultAsync(i => i.GithubIssueId == ghIssue.Id, stoppingToken);

                    if (existingIssue is null)
                    {
                        var repoName = ExtractRepoName(ghIssue.RepositoryUrl);

                        existingIssue = new Issue
                        {
                            GithubIssueId = ghIssue.Id,
                            Repository = repoName,
                            IssueNumber = ghIssue.Number,
                            Title = ghIssue.Title,
                            Url = ghIssue.HtmlUrl,
                            State = ghIssue.State,
                            Labels = string.Join(",", ghIssue.Labels.Select(l => l.Name)),
                            GithubCreatedAt = ghIssue.CreatedAt,
                            GithubUpdatedAt = ghIssue.UpdatedAt,
                            FirstSeenAt = DateTimeOffset.UtcNow,
                            LastSeenAt = DateTimeOffset.UtcNow
                        };

                        db.Issues.Add(existingIssue);
                        await db.SaveChangesAsync(stoppingToken);
                        
                        _logger.LogInformation("Stored new issue: {Title}", existingIssue.Title);
                    }
                    else
                    {
                        existingIssue.Title = ghIssue.Title;
                        existingIssue.Url = ghIssue.HtmlUrl;
                        existingIssue.State = ghIssue.State;
                        existingIssue.Labels = string.Join(",", ghIssue.Labels.Select(l => l.Name));
                        existingIssue.GithubUpdatedAt = ghIssue.UpdatedAt;
                        existingIssue.LastSeenAt = DateTimeOffset.UtcNow;
                    }
                    
                    // Ensure SearchIssue link exists
                    var existingLink = await db.SearchIssues.FirstOrDefaultAsync(
                        si => si.SearchId == search.Id && si.IssueId == existingIssue.Id,
                        stoppingToken);

                    if (existingLink is null)
                    {
                        db.SearchIssues.Add(new SearchIssue
                        {
                            SearchId = search.Id,
                            IssueId = existingIssue.Id,
                            DiscoveredAt = DateTimeOffset.UtcNow
                        });
                    }
                }
            }
            
            // // For now, hardcode one sample search
            // const string searchName = "C# good first issues";
            // string rawQuery = "label:\"good first issue\" language: c# state:open";


            // // Ensure search exists
            // var search = await db.Searches.FirstOrDefaultAsync(
            //     s => s.Query == rawQuery,
            //     stoppingToken);
            
            /*if (search is null)
            {
                search = new Search
                {
                    Name = searchName,
                    Query = rawQuery,
                    Enabled = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                db.Searches.Add(search);
                await db.SaveChangesAsync(stoppingToken);
            }*/

    }

    public sealed class GitHubIssueQueryBuilder
    {
        private readonly List<string> _labels = new();
        private readonly List<string> _languages = new();
        private readonly List<string> _rawTokens = new();
        private string? _state;
        private bool _requireNoAssignee;
        private string? _sortField;
        private string? _sortDirection;

        public static GitHubIssueQueryBuilder ForSearch(Search search)
        {
            if (search == null) throw new ArgumentNullException();

            var builder = new GitHubIssueQueryBuilder();

            builder.RequireState("open");

            if (!string.IsNullOrWhiteSpace(search.Labels))
            {
                var labels = search.Labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                builder.IncludeLabels(labels);
            }

            if (!string.IsNullOrWhiteSpace(search.Languages))
            {
                var languages = search.Languages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                builder.IncludeLanguages(languages);
            }

            if (!string.IsNullOrWhiteSpace(search.Query))
            {
                builder.WithRawTerms(search.Query);
            }

            return builder;

        }

        public GitHubIssueQueryBuilder RequireState(string state = "open")
        {
            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentException("State cannot be empty.", nameof(state));
            }

            state = state.Trim().ToLowerInvariant();

            if (state is not ("open" or "closed" or "all"))
            {
                throw new ArgumentException($"Unsupported state '{state}'.");
            }

            _state = state;
            return this;
        }

        public GitHubIssueQueryBuilder RequireNoAssignee()
        {
            _requireNoAssignee = true;
            return this;
        }

        public GitHubIssueQueryBuilder IncludeLabels(IEnumerable<string> labels)
        {
            if (labels == null)
            {
                return this;
            }

            foreach (var label in labels)
            {
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                var trimmed = label.Trim();

                if (!_labels.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                {
                    _labels.Add(trimmed);
                }
            }

            return this;
        }

        public GitHubIssueQueryBuilder IncludeLanguages(IEnumerable<string> languages)
        {
            if (languages == null)
            {
                return this;
            }

            foreach (var language in languages)
            {
                if (string.IsNullOrWhiteSpace(language))
                {
                    continue;
                }

                var trimmed = language.Trim();

                if (!_languages.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                {
                    _languages.Add(trimmed);
                }
            }

            return this;
        }

        public GitHubIssueQueryBuilder WithRawTerms(string rawQuery)
        {
            if (string.IsNullOrWhiteSpace(rawQuery))
            {
                return this;
            }
            
            _rawTokens.Add(rawQuery.Trim());
            return this;
        }

        public GitHubIssueQueryBuilder SortBy(string field, string direction = "desc")
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentException("Sort field is required.", nameof(field));
            }

            field = field.Trim().ToLowerInvariant();
            direction = string.IsNullOrWhiteSpace(direction) ? "desc" : direction.Trim().ToLowerInvariant();

            if (direction is not ("asc" or "desc"))
            {
                throw new ArgumentException("Sort direction must be 'asc' of 'desc'.", nameof(direction));
            }

            _sortField = field;
            _sortDirection = direction;

            return this;
        }

        public string Build()
        {
            var tokens = new List<string>();

            if (!string.IsNullOrWhiteSpace(_state))
            {
                tokens.Add($"state:{_state}");
            }

            if (_requireNoAssignee)
            {
                tokens.Add("no:assignee");
            }

            foreach (var label in _labels)
            {
                var escaped = label.Replace("\"", "\\\"");
                tokens.Add($"label:\"{escaped}\"");
            }

            foreach (var language in _languages)
            {
                var escaped = language.Replace("\"", "\\\"");
                tokens.Add($"language:{escaped}");
            }
            
            tokens.AddRange(_rawTokens);

            if (!string.IsNullOrWhiteSpace(_sortField))
            {
                tokens.Add($"sort:{_sortField}");
                tokens.Add($"order:{_sortDirection ?? "desc"}");
            }

            return string.Join(' ', tokens.Where(t => !string.IsNullOrWhiteSpace(t)));
        }
    }
    
}
