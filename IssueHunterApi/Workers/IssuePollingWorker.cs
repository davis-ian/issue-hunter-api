using System.Reflection.Metadata;
using IssueHunter.Data;
using IssueHunter.Dtos.GitHub;
using IssueHunter.Models;
using IssueHunter.Services;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Workers;

public class IssuePollingWorker : BackgroundService
{
    private readonly ILogger<IssuePollingWorker> _logger;
    private readonly GitHubService _github;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    
    public IssuePollingWorker(ILogger<IssuePollingWorker> logger, GitHubService github, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _logger = logger;
        _github = github;
        _dbContextFactory = dbContextFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Issue polling worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Polling GitHub...");

            // For now, hardcode one sample search
            const string searchName = "C# good first issues";
            string query = "label:\"good first issue\" language: c# state:open";


            await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken);
            // Ensure search exists
            var search = await db.Searches.FirstOrDefaultAsync(
                s => s.Query == query,
                stoppingToken);
            
            if (search is null)
            {
                search = new Search
                {
                    Name = searchName,
                    Query = query,
                    Enabled = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                db.Searches.Add(search);
                await db.SaveChangesAsync(stoppingToken);
            }

            
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
            
            Console.WriteLine(result);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
    
    private static string ExtractRepoName(string repositoryUrl)
    {
        var parts = repositoryUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            return $"{parts[^2]}/{parts[^1]}";
        }

        return repositoryUrl;
    }
}
