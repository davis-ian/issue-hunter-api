using System.Text.Json.Serialization;

namespace IssueHunter.Dtos.GitHub;

public class GitHubUserDto
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";
}