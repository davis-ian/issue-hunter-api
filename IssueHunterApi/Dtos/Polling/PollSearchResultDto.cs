namespace IssueHunter.Dtos.Polling;

public class PollSearchResultDto
{
    public int SearchId { get; set; }
    public string SearchName { get; set; } = "";
    
    public bool Succeeded { get; set; }
    public string Error { get; set; } = "";
    
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    
    public int FetchedCount { get; set;  }
    public int NewCount { get; set; }
    public int UpdatedCount { get; set; }
    public int LinkedCount { get; set; }
    
    public DateTimeOffset? NextRunAfter { get; set; }
}