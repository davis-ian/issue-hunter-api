using IssueHunter.Data;
using IssueHunter.Dtos.Polling;
using IssueHunter.Dtos.Search;
using IssueHunter.Models;
using IssueHunter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/searches")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SearchController(AppDbContext db, IServiceScopeFactory serviceScopeFactory)
    {
        _db = db;
        _serviceScopeFactory = serviceScopeFactory;
    }

    [HttpGet]
    public async Task<ActionResult<List<SearchResponseDto>>> Get()
    {
        var searches = await _db.Searches
            .OrderByDescending(s => s.Priority)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Ok(searches.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SearchResponseDto>> GetById(int id)
    {
        var search = await _db.Searches.FirstOrDefaultAsync(s => s.Id == id);
        if (search is null) return NotFound();

        return Ok(ToResponse(search));
    }

    [HttpPost]
    public async Task<ActionResult<SearchResponseDto>> Create([FromBody] CreateSearchRequestDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var now = DateTimeOffset.UtcNow;

        var search = new Search
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? "",
            Labels = dto.Labels?.Trim() ?? "",
            Languages = dto.Languages?.Trim() ?? "",
            Query = dto.Query?.Trim() ?? "",
            IntervalMinutes = dto.IntervalMinutes,
            Enabled = dto.Enabled,
            Priority = dto.Priority,
            CreatedAt = now,
            UpdatedAt = now,
            NextRunAfter = now
        };

        _db.Searches.Add(search);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = search.Id }, ToResponse(search));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SearchResponseDto>> Update(int id, [FromBody] UpdateSearchRequestDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var search = await _db.Searches.FirstOrDefaultAsync(s => s.Id == id);
        if (search is null) return NotFound();

        search.Name = dto.Name.Trim();
        search.Description = dto.Description?.Trim() ?? "";
        search.Labels = dto.Labels?.Trim() ?? "";
        search.Languages = dto.Languages?.Trim() ?? "";
        search.Query = dto.Query?.Trim() ?? "";
        search.IntervalMinutes = dto.IntervalMinutes;
        search.Enabled = dto.Enabled;
        search.Priority = dto.Priority;
        search.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToResponse(search));
    }

    [HttpPost("{id:int}/poll")]
    public async Task<ActionResult<PollRunSummaryDto>> PollSearch(int id, CancellationToken ct)
    {
        
        using var scope = _serviceScopeFactory.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IIssuePollingOrchestrator>();
        var result = await orchestrator.PollSearchAsync(id, ct);
        
        return Ok(result);
    }
    
    private static SearchResponseDto ToResponse(Search s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        Labels = s.Labels,
        Languages = s.Languages,
        Query = s.Query,
        IntervalMinutes = s.IntervalMinutes,
        Enabled = s.Enabled,
        Priority = s.Priority,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
        LastPolledAt = s.LastPolledAt,
        NextRunAfter = s.NextRunAfter,
        LastResultCount = s.LastResultCount,
        LastError = s.LastError
    };
}