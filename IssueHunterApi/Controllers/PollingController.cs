
using IssueHunter.Dtos.Polling;
using IssueHunter.Services;
using Microsoft.AspNetCore.Mvc;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/polling")]
public class PollingController : ControllerBase
{

    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public PollingController(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    
    [HttpPost("all")]
    public async Task<ActionResult<PollRunSummaryDto>> PollAllDueSearches(CancellationToken ct)
    {
        
        using var scope = _serviceScopeFactory.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<IIssuePollingOrchestrator>();
        var result = await orchestrator.PollDueSearchesAsync(ct);

        return Ok(result);
    }
}