using System.ComponentModel.DataAnnotations;

namespace IssueHunter.Dtos.Search;

public class CreateSearchRequestDto
{

    [MaxLength(500)]
    public string Name { get; set; } = "";
    
    [MaxLength(500)]
    public string Description { get; set; } = "";
    
    [MaxLength(300)]
    public string Labels { get; set; } = "";
    
    [MaxLength(300)]
    public string Languages { get; set; } = "";
    
    [MaxLength(500)]
    public string Query { get; set; } = "";
    
    [Range(5, 1440)]
    public int IntervalMinutes { get; set; } = 60;
    public bool Enabled { get; set; } = true;
    
    [Range(-100, 100)]
    public int Priority { get; set; } = 0;
}