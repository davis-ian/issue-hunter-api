namespace IssueHunter.Models;

public class SearchIssue
{
    public int Id { get; set; }
    
    public int SearchId { get; set; }
    public Search Search { get; set; } = null;
    
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null;
    
    public DateTimeOffset DiscoveredAt { get; set; }
}