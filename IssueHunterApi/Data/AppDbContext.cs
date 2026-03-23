using IssueHunter.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueHunter.Data;

public class AppDbContext : DbContext
{
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Repo> Repos => Set<Repo>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Issue>()
            .HasIndex(i => i.GithubIssueId)
            .IsUnique();
    }
}