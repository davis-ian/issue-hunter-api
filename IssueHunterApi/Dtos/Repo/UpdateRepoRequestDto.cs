using System.ComponentModel.DataAnnotations;

namespace IssueHunter.Dtos.Search;

public class UpdateRepoRequestDto
{

    [Range(5, 1440)]
    public int IntervalMinutes { get; set; } = 60;
    
    public bool Enabled { get; set; }
    
    [Range(-100, 100)]
    public int Priority { get; set; }
}