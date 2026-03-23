using System.ComponentModel.DataAnnotations;

namespace IssueHunter.Dtos.Search;

public class CreateRepoRequestDto
{

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = "";
    
    [Range(5, 1440)]
    public int IntervalMinutes { get; set; } = 60;
    public bool Enabled { get; set; } = true;
    
    [Range(-100, 100)]
    public int Priority { get; set; } = 0;
}