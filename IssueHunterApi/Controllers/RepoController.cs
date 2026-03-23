using IssueHunter.Data;
using IssueHunter.Dtos;
using IssueHunter.Dtos.Polling;
using IssueHunter.Dtos.Search;
using IssueHunter.Models;
using IssueHunter.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/repos")]
public class RepoController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly GitHubService _gitHub;
    
    public RepoController(
        AppDbContext db,
        GitHubService gitHub
        )
    {
        _db = db;
        _gitHub = gitHub;
    }

    [HttpGet]
    public async Task<ActionResult<List<Repo>>> Get()
    {
        var repos = await _db.Repos
            .OrderByDescending(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Ok(repos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RepoResponseDto>> GetById(int id)
    {
        var repo = await _db.Repos.FirstOrDefaultAsync(s => s.Id == id);
        if (repo is null) return NotFound();

        return Ok(repo);
    }

    [HttpPost]
    public async Task<ActionResult<RepoResponseDto>> Create([FromBody] CreateRepoRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!TryParseGitHubRepoUrl(dto.Url, out var owner, out var name))
            return BadRequest("Invalid GitHub repo URL. Expected format: https://github.com/owner/repo");

        var fullName = $"{owner}/{name}";

        var existing = await _db.Repos.FirstOrDefaultAsync(r => r.FullName == fullName, ct);
        if (existing is not null)
            return Conflict($"Repo '{fullName}' is already on the dashboard");

        var ghRepo = await _gitHub.GetRepositoryAsync(owner, name, ct);
        
        var now = DateTimeOffset.UtcNow;
        var repo = new Repo
        {
            Owner = owner,
            Name = name,
            FullName = fullName,
            Description = ghRepo.Description ?? "",
            HtmlUrl = ghRepo.HtmlUrl ?? dto.Url.Trim(),
            Language = ghRepo.Language ?? "",
            StarCount = ghRepo.StargazersCount,
            OpenIssueCount = ghRepo.OpenIssuesCount,
            Archived = ghRepo.Archived,
            
            IntervalMinutes = dto.IntervalMinutes,
            Enabled = dto.Enabled,
            Priority = dto.Priority,
            CreatedAt = now,
            UpdatedAt = now,
            NextSyncAfter = now
        };

        _db.Repos.Add(repo);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = repo.Id }, repo);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RepoResponseDto>> Update(int id, [FromBody] UpdateRepoRequestDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
    
        var repo = await _db.Repos.FirstOrDefaultAsync(s => s.Id == id);
        if (repo is null) return NotFound();

        repo.IntervalMinutes = dto.IntervalMinutes;
        repo.Enabled = dto.Enabled;
        repo.Priority = dto.Priority;
    
        await _db.SaveChangesAsync();
    
        return Ok(repo);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRepo(int id)
    {
        var repo = await _db.Repos.FindAsync(id);

        if (repo is null)
            return NotFound("Repo not found");

        _db.Repos.Remove(repo);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // [HttpPost("{id:int}/poll")]
    // public async Task<ActionResult<PollRunSummaryDto>> PollSearch(int id, CancellationToken ct)
    // {
    //     
    //     var result = await _issuePollingOrchestrator.PollSearchAsync(id, ct);
    //     
    //     return Ok(result);
    // }
    

    private static bool TryParseGitHubRepoUrl(string input, out string owner, out string name)
    {
        owner = "";
        name = "";

        if (string.IsNullOrWhiteSpace(input)) return false;
        if (!Uri.TryCreate(input.Trim(), UriKind.Absolute, out var uri)) return false;
        if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)) return false;

        var parts = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return false;

        owner = parts[0];
        name = parts[1].Replace(".git", "", StringComparison.OrdinalIgnoreCase);
        return !string.IsNullOrWhiteSpace(owner) && !string.IsNullOrWhiteSpace(name);
    }
}