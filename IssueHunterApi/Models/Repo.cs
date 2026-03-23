namespace IssueHunter.Models;

public class Repo
{
    public int Id { get; set; }
    public string Owner { get; set; } = "";
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";

    public string Description { get; set; } = "";
    public string HtmlUrl { get; set; } = "";
    public string Language { get; set; } = "";
    public int StarCount { get; set; }
    public int OpenIssueCount { get; set; }
    public bool Archived { get; set; }


    public bool Enabled { get; set; } = true;
    public int IntervalMinutes { get; set; } = 60;
    public int Priority { get; set; } = 0;
    
    public DateTimeOffset? LastSyncedAt { get; set;  }
    public DateTimeOffset? NextSyncAfter { get; set; }
    public int LastResultCount { get; set; } = 0;
    public string LastError { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Issue> Issues { get; set; } = new();
}