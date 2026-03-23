using System.Threading.RateLimiting;
using IssueHunter.Data;
using IssueHunter.Dtos.Polling;
using IssueHunter.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Services;

public class IssuePollingOrchestrator : IIssuePollingOrchestrator
{
    private readonly ILogger<IssuePollingOrchestrator> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IGitHubIssueIngestionService _ingestion;
    private readonly SemaphoreSlim _pollLock = new(1, 1);

    public IssuePollingOrchestrator(
        ILogger<IssuePollingOrchestrator> logger,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IGitHubIssueIngestionService ingestion)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _ingestion = ingestion;
    }

    // public async Task<PollRunSummaryDto> PollDueSearchesAsync(CancellationToken stoppingToken)
    // {
    //     if (!await _pollLock.WaitAsync(TimeSpan.Zero, stoppingToken))
    //     {
    //         _logger.LogInformation("Poll already in progress, skipping.");
    //         return new PollRunSummaryDto
    //         {
    //             StartedAt = DateTimeOffset.UtcNow,
    //             CompletedAt = DateTimeOffset.UtcNow,
    //             SearchesAttempted = 0,
    //         };
    //     }
    //
    //     var startedAt = DateTimeOffset.UtcNow;
    //     var summary = new PollRunSummaryDto { StartedAt = startedAt };
    //
    //     try
    //     {
    //         await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken);
    //
    //         var searches = await db.Searches
    //             .Where(s => s.Enabled)
    //             .OrderByDescending(s => s.Priority)
    //             .ToListAsync(stoppingToken);
    //
    //         var dueSearches = searches
    //             .Where(s => s.NextRunAfter == null || s.NextRunAfter <= startedAt)
    //             .ToList();
    //
    //         summary.SearchesAttempted = dueSearches.Count;
    //
    //         if (dueSearches.Count == 0)
    //         {
    //             _logger.LogInformation("No searches due for polling.");
    //             return summary;
    //         }
    //
    //         foreach (var search in dueSearches)
    //         {
    //             var result = await ProcessSearchAsync(db, search, stoppingToken);
    //             summary.SearchResults.Add(result);
    //
    //             if (result.Succeeded)
    //                 summary.SearchesSucceeded++;
    //             else
    //                 summary.SearchesFailed++;
    //
    //             summary.IssuesFetched += result.FetchedCount;
    //             summary.IssuesNew += result.NewCount;
    //             summary.IssuesUpdated += result.UpdatedCount;
    //             summary.LinksCreated += result.LinkedCount;
    //         }
    //     }
    //     finally
    //     {
    //         _pollLock.Release();
    //         summary.CompletedAt = DateTimeOffset.UtcNow;
    //     }
    //
    //     return summary;
    // }

    // public async Task<PollRunSummaryDto> PollSearchAsync(int searchId, CancellationToken stoppingToken)
    // {
    //     if (!await _pollLock.WaitAsync(TimeSpan.Zero, stoppingToken))
    //     {
    //         _logger.LogInformation("Poll already in progress, skipping.");
    //         return new PollRunSummaryDto
    //         {
    //             StartedAt = DateTimeOffset.UtcNow,
    //             CompletedAt = DateTimeOffset.UtcNow,
    //             SearchesAttempted = 0,
    //         };
    //     }
    //
    //     var startedAt = DateTimeOffset.UtcNow;
    //     var summary = new PollRunSummaryDto { StartedAt = startedAt };
    //
    //     try
    //     {
    //         await using var db = await _dbContextFactory.CreateDbContextAsync(stoppingToken);
    //
    //         var search = await db.Searches.FirstOrDefaultAsync(s => s.Id == searchId, stoppingToken);
    //         if (search is null)
    //         {
    //             _logger.LogWarning("Search with id {SearchId} not found.", searchId);
    //             return summary;
    //         }
    //
    //         summary.SearchesAttempted = 1;
    //
    //         var result = await ProcessSearchAsync(db, search, stoppingToken);
    //         summary.SearchResults.Add(result);
    //
    //         if (result.Succeeded)
    //             summary.SearchesSucceeded++;
    //         else
    //             summary.SearchesFailed++;
    //
    //         summary.IssuesFetched += result.FetchedCount;
    //         summary.IssuesNew += result.NewCount;
    //         summary.IssuesUpdated += result.UpdatedCount;
    //         summary.LinksCreated += result.LinkedCount;
    //     }
    //     finally
    //     {
    //         _pollLock.Release();
    //         summary.CompletedAt = DateTimeOffset.UtcNow;
    //     }
    //
    //     return summary;
    // }

    // private async Task<PollSearchResultDto> ProcessSearchAsync(AppDbContext db, Search search, CancellationToken ct)
    // {
    //     var startedAt = DateTimeOffset.UtcNow;
    //     var result = new PollSearchResultDto
    //     {
    //         SearchId = search.Id,
    //         SearchName = search.Name,
    //         StartedAt = startedAt,
    //     };
    //
    //     try
    //     {
    //         var ingestResult = await _ingestion.IngestSearchAsync(db, search, ct);
    //
    //         result.FetchedCount = ingestResult.FetchedCount;
    //         result.NewCount = ingestResult.NewCount;
    //         result.UpdatedCount = ingestResult.UpdatedCount;
    //         result.LinkedCount = ingestResult.LinkedCount;
    //         result.Succeeded = true;
    //
    //         search.LastResultCount = ingestResult.FetchedCount;
    //         search.LastError = "";
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to poll search {SearchName} (id={SearchId})", search.Name, search.Id);
    //
    //         result.Succeeded = false;
    //         result.Error = ex.Message;
    //
    //         search.LastResultCount = 0;
    //         search.LastError = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
    //     }
    //     finally
    //     {
    //         search.LastPolledAt = DateTimeOffset.UtcNow;
    //         search.NextRunAfter = DateTimeOffset.UtcNow.AddMinutes(search.IntervalMinutes);
    //         result.NextRunAfter = search.NextRunAfter;
    //
    //         await db.SaveChangesAsync(ct);
    //     }
    //
    //     result.CompletedAt = DateTimeOffset.UtcNow;
    //     return result;
    // }
}