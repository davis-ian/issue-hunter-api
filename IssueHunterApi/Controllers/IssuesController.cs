using IssueHunter.Data;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Get()
    {
        return Ok(_db.Issues.ToList());
    }
}