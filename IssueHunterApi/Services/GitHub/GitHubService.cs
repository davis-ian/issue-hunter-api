using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using IssueHunter.Dtos.GitHub;

namespace IssueHunter.Services;

public class GitHubService
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };


    public GitHubService(HttpClient client)
    {
        _client = client;
        
        _client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("IssueHunter", "1.0"));
        
        _client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
    }

    public async Task<GitHubSearchResponseDto> SearchIssuesAsync(string query)
    {
        int pageSize = 100;
        string sortBy = "created";
        string order = "desc";
        
        var url =
            $"https://api.github.com/search/issues?q={Uri.EscapeDataString(query)}&sort={sortBy}&order={order}&per_page={pageSize}";
        
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<GitHubSearchResponseDto>(json, _jsonOptions);

        if (result is null)
        {
            throw new InvalidOperationException("Failed to deserialize GitHub search response.");
        }
            
        return result;
    }

    public async Task<GitHubRepositoryDto?> GetRepositoryAsync(string owner, string name,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Owner is required.", nameof(owner));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Repository name is required.", nameof(name));

        var url = $"https://api.github.com/repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(name)}";

        var response = await _client.GetAsync(url, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);

        var result = JsonSerializer.Deserialize<GitHubRepositoryDto>(json, _jsonOptions);

        if (result is null)
        {
            throw new InvalidOperationException("Failed to deserialize GitHub repository response.");
        }

        return result;
    }
}