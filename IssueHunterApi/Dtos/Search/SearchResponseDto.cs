namespace IssueHunter.Dtos.Search;

public class SearchResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Labels { get; set; }
    public string Languages { get; set; }
    public string Query { get; set; }
    public int IntervalMinutes { get; set; }
    public bool Enabled { get; set; }
    public int Priority { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastPolledAt { get; set; }
    public DateTimeOffset? NextRunAfter { get; set; }
    public int LastResultCount { get; set; }
    public string LastError { get; set; } = "";
}