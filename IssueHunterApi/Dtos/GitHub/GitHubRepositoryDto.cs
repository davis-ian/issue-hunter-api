using System.Text.Json.Serialization;

namespace IssueHunter.Dtos.GitHub;

public class GitHubRepositoryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("full_name")]
    public string FullName { get; set; }
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; set; }

    [JsonPropertyName("open_issues_count")]
    public int OpenIssuesCount { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("owner")]
    public GitHubRepositoryOwnerDto Owner { get; set; } = new();
}

public class GitHubRepositoryOwnerDto
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = "";
}