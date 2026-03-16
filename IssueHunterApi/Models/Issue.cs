namespace IssueHunter.Models;

public class Issue
{
    public int Id { get; set; }

    // GitHub identifiers
    public long GithubIssueId { get; set; }
    public string Repository { get; set; } = "";
    public int IssueNumber { get; set; }

    // Metadata
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string State { get; set; } = "";
    
    // Labels serialized
    public string Labels { get; set; } = "";
    
    // Timestamps from Github
    public DateTimeOffset GithubCreatedAt { get; set; }
    public DateTimeOffset GithubUpdatedAt { get; set; }
    
    // When our system discovered it
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    
    // Navigation
    public List<SearchIssue> SearchIssues { get; set; } = new();
}


