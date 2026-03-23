using IssueHunter.Data;
using IssueHunter.Dtos.Issue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/issues")]
public class IssuesController : ControllerBase
{
  
    private readonly AppDbContext _db;

    public IssuesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult> Get(int pageNumber = 0, int pageSize = 10, string search = null)
    {
        var issuesQuery = _db.Issues
            .OrderByDescending(i => i.Id);

        var total = await issuesQuery.CountAsync();

        var pagedIssues = issuesQuery
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new { Total = total, Results = pagedIssues });
    }

}