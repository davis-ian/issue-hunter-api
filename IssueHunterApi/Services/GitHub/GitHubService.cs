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
        var url =
            $"https://api.github.com/search/issues?q={Uri.EscapeDataString(query)}&sort=created&order=desc";
        
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
}