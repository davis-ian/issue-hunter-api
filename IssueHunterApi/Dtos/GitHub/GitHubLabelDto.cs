using System.Text.Json.Serialization;

namespace IssueHunter.Dtos.GitHub;

public class GitHubLabelDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}