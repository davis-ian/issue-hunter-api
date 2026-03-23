using IssueHunter.Data;
using IssueHunter.Dtos.GitHub;
using IssueHunter.Dtos.Polling;
using IssueHunter.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Services;

public class GitHubIssueIngestionService : IGitHubIssueIngestionService
{
    private readonly GitHubService _github;

    public GitHubIssueIngestionService(GitHubService github)
    {
        _github = github;
    }

    // public async Task<PollSearchResultDto> IngestSearchAsync(AppDbContext db, Search search, CancellationToken stoppingToken)
    // {
    //     var result = new PollSearchResultDto
    //     {
    //         SearchId = search.Id,
    //         SearchName = search.Name,
    //         StartedAt = DateTimeOffset.UtcNow,
    //     };
    //
    //     var query = GitHubIssueQueryBuilder.ForSearch(search)
    //         .RequireNoAssignee()
    //         .Build();
    //
    //     var githubResult = await _github.SearchIssuesAsync(query);
    //
    //     var githubIssues = githubResult.Items
    //         .Where(x => x.PullRequest is null)
    //         .ToList();
    //
    //     result.FetchedCount = githubIssues.Count;
    //
    //     if (githubIssues.Count == 0)
    //     {
    //         result.Succeeded = true;
    //         result.CompletedAt = DateTimeOffset.UtcNow;
    //         return result;
    //     }
    //
    //     var githubIds = githubIssues.Select(i => i.Id).ToList();
    //
    //     var existingIssues = await db.Issues
    //         .Where(i => githubIds.Contains(i.GithubIssueId))
    //         .ToDictionaryAsync(i => i.GithubIssueId, stoppingToken);
    //
    //     var newIssues = new List<Issue>(githubIssues.Count);
    //     var updatedCount = 0;
    //
    //     foreach (var ghIssue in githubIssues)
    //     {
    //         if (existingIssues.TryGetValue(ghIssue.Id, out var existing))
    //         {
    //             existing.Title = ghIssue.Title;
    //             existing.Url = ghIssue.HtmlUrl;
    //             existing.State = ghIssue.State;
    //             existing.Labels = string.Join(",", ghIssue.Labels.Select(l => l.Name));
    //             existing.GithubUpdatedAt = ghIssue.UpdatedAt;
    //             existing.LastSeenAt = DateTimeOffset.UtcNow;
    //             updatedCount++;
    //         }
    //         else
    //         {
    //             newIssues.Add(new Issue
    //             {
    //                 GithubIssueId = ghIssue.Id,
    //                 Repository = ExtractRepoName(ghIssue.RepositoryUrl),
    //                 IssueNumber = ghIssue.Number,
    //                 Title = ghIssue.Title,
    //                 Url = ghIssue.HtmlUrl,
    //                 State = ghIssue.State,
    //                 Labels = string.Join(",", ghIssue.Labels.Select(l => l.Name)),
    //                 GithubCreatedAt = ghIssue.CreatedAt,
    //                 GithubUpdatedAt = ghIssue.UpdatedAt,
    //                 FirstSeenAt = DateTimeOffset.UtcNow,
    //                 LastSeenAt = DateTimeOffset.UtcNow
    //             });
    //         }
    //     }
    //
    //     if (newIssues.Count > 0)
    //     {
    //         db.Issues.AddRange(newIssues);
    //     }
    //
    //     var processedIssues = existingIssues.Values
    //         .Concat(newIssues)
    //         .ToList();
    //
    //     var existingIssueIds = processedIssues
    //         .Where(i => i.Id > 0)
    //         .Select(i => i.Id)
    //         .ToList();
    //
    //     var existingLinkIssueIds = await db.SearchIssues
    //         .Where(si => si.SearchId == search.Id && existingIssueIds.Contains(si.IssueId))
    //         .Select(si => si.IssueId)
    //         .ToListAsync(stoppingToken);
    //
    //     var existingLinkSet = new HashSet<int>(existingLinkIssueIds);
    //     var newLinks = processedIssues
    //         .Where(issue => issue.Id == 0 || !existingLinkSet.Contains(issue.Id))
    //         .Select(issue => new SearchIssue
    //         {
    //             SearchId = search.Id,
    //             Issue = issue,
    //             DiscoveredAt = DateTimeOffset.UtcNow
    //         })
    //         .ToList();
    //
    //     if (newLinks.Count > 0)
    //     {
    //         db.SearchIssues.AddRange(newLinks);
    //     }
    //
    //     result.NewCount = newIssues.Count;
    //     result.UpdatedCount = updatedCount;
    //     result.LinkedCount = newLinks.Count;
    //     result.Succeeded = true;
    //     result.CompletedAt = DateTimeOffset.UtcNow;
    //
    //     return result;
    // }

    private string ExtractRepoName(string repositoryUrl)
    {
        var parts = repositoryUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            return $"{parts[^2]}/{parts[^1]}";
        }

        return repositoryUrl;
    }
    
    public sealed class GitHubIssueQueryBuilder
    {
        private readonly List<string> _labels = new();
        private readonly List<string> _languages = new();
        private readonly List<string> _rawTokens = new();
        private string? _state;
        private bool _requireNoAssignee;
    
        // public static GitHubIssueQueryBuilder ForSearch(Search search)
        // {
        //     if (search == null) throw new ArgumentNullException();
        //
        //     var builder = new GitHubIssueQueryBuilder();
        //
        //     builder.RequireState("open");
        //
        //     if (!string.IsNullOrWhiteSpace(search.Labels))
        //     {
        //         var labels = search.Labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //         builder.IncludeLabels(labels);
        //     }
        //
        //     if (!string.IsNullOrWhiteSpace(search.Languages))
        //     {
        //         var languages = search.Languages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        //         builder.IncludeLanguages(languages);
        //     }
        //
        //     if (!string.IsNullOrWhiteSpace(search.Query))
        //     {
        //         builder.WithRawTerms(search.Query);
        //     }
        //
        //     return builder;
        //
        // }
    
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
    
            return string.Join(' ', tokens.Where(t => !string.IsNullOrWhiteSpace(t)));
        }
    }}
