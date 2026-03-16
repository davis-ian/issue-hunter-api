using IssueHunter.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Data;

public class AppDbContext : DbContext
{
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Search> Searches => Set<Search>();
    public DbSet<SearchIssue> SearchIssues => Set<SearchIssue>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Issue>()
            .HasIndex(i => i.GithubIssueId)
            .IsUnique();

        modelBuilder.Entity<SearchIssue>()
            .HasIndex(si => new { si.SearchId, si.IssueId })
            .IsUnique();
    }
}