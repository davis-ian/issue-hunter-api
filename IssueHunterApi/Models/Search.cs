namespace IssueHunter.Models;

public class Search
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Labels { get; set; } = "";
    public string Languages { get; set; } = "";
    public int IntervalMinutes { get; set; } = 60;

    // Example
    // "label:\"good first issue\" language:c#"
    public string Query { get; set; } = "";
    public bool Enabled { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastPolledAt { get; set; }
    public DateTimeOffset? NextRunAfter { get; set; }
    public int LastResultCount { get; set; } = 0;
    public string LastError { get; set; } = "";

    public int Priority { get; set; } = 0;
    // Navigation
    public List<SearchIssue> SearchIssues { get; set; } = new();

}