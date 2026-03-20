namespace IssueHunter.Dtos.Polling;

public class PollRunSummaryDto
{
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    
    public int SearchesAttempted { get; set; }
    public int SearchesSucceeded { get; set; }
    public int SearchesFailed { get; set; }
    
    public int IssuesFetched { get; set; }
    public int IssuesNew { get; set; }
    public int IssuesUpdated { get; set; }
    public int LinksCreated { get; set; }

    public List<PollSearchResultDto> SearchResults { get; set; } = new();
}