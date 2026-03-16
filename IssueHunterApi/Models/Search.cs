namespace IssueHunter.Models;

public class Search
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Example
    // "label:\"good first issue\" language:c#"
    public string Query { get; set; } = "";
    public bool Enabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTimeOffset? LastPolledAt { get; set; }
    
    // Navigation
    public List<SearchIssue> SearchIssues { get; set; } = new();

}