using IssueHunter.Dtos.Polling;

namespace IssueHunter.Services;

public interface IIssuePollingOrchestrator
{
    Task<PollRunSummaryDto> PollDueSearchesAsync(CancellationToken ct);
    Task<PollRunSummaryDto> PollSearchAsync(int searchId, CancellationToken ct);
}