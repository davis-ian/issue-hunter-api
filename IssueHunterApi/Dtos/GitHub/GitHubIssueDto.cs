using System.Text.Json.Serialization;

namespace IssueHunter.Dtos.GitHub;

public class GitHubIssueDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; set; } = "";

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("state")]
    public string State { get; set; } = "";

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = "";

    [JsonPropertyName("repository_url")]
    public string RepositoryUrl { get; set; } = "";

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("user")]
    public GitHubUserDto? User { get; set; }

    [JsonPropertyName("labels")]
    public List<GitHubLabelDto> Labels { get; set; } = new();

    // Present when the result is actually a pull request
    [JsonPropertyName("pull_request")]
    public GitHubPullRequestDto? PullRequest { get; set; }
}