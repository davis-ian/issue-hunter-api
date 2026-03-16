using System.Text.Json.Serialization;

namespace IssueHunter.Dtos.GitHub;

public class GitHubSearchResponseDto
{
    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("incomplete_results")]
    public bool IncompleteResults { get; set; }

    [JsonPropertyName("items")]
    public List<GitHubIssueDto> Items { get; set; } = new();
}