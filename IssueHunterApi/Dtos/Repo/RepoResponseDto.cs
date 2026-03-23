namespace IssueHunter.Dtos;

public class RepoResponseDto
{
    public int Id { get; set; }
    public string Owner { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }

    public string Description { get; set; }
    public string HtmlUrl { get; set; }
    public string Language { get; set; }
    public int StarCount { get; set; }
    public int OpenIssueCount { get; set; }
    public bool Archived { get; set; }


    public bool Enabled { get; set; }
    public int IntervalMinutes { get; set; } 
    public int Priority { get; set; } 
    
    public DateTimeOffset? LastSyncedAt { get; set;  }
    public DateTimeOffset? NextSyncAfter { get; set; }
    public int LastResultCount { get; set; }
    public string LastError { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<Models.Issue> Issues { get; set; } = new();
}