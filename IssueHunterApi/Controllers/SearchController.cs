using IssueHunter.Data;
using IssueHunter.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueHunter.Controllers;

[ApiController]
[Route("api/searches")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_db.Searches.ToList());
    }

    [HttpPost]
    public IActionResult Create(Search search)
    {
        _db.Searches.Add(search);
        _db.SaveChanges();
        return Ok(search);
    }
}