
using IssueHunter.Dtos.Polling;
using IssueHunter.Services;
using Microsoft.AspNetCore.Mvc;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/polling")]
public class PollingController : ControllerBase
{

    private readonly IIssuePollingOrchestrator _issuePollingOrchestrator;
    
    public PollingController(IIssuePollingOrchestrator issuePollingOrchestrator)
    {
        _issuePollingOrchestrator = issuePollingOrchestrator;
    }
    
    
    [HttpPost("all")]
    public async Task<ActionResult<PollRunSummaryDto>> PollAllDueSearches(CancellationToken ct)
    {
        
        var result = await _issuePollingOrchestrator.PollDueSearchesAsync(ct);

        return Ok(result);
    }
}