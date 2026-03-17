using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueHunter.Migrations
{
    /// <inheritdoc />
    public partial class SearchProfileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GithubIssueId = table.Column<long>(type: "INTEGER", nullable: false),
                    Repository = table.Column<string>(type: "TEXT", nullable: false),
                    IssueNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    Labels = table.Column<string>(type: "TEXT", nullable: false),
                    GithubCreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    GithubUpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    FirstSeenAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Searches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Labels = table.Column<string>(type: "TEXT", nullable: false),
                    Languages = table.Column<string>(type: "TEXT", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Query = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastPolledAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    NextRunAfter = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastResultCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastError = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Searches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SearchId = table.Column<int>(type: "INTEGER", nullable: false),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscoveredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchIssues_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SearchIssues_Searches_SearchId",
                        column: x => x.SearchId,
                        principalTable: "Searches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_GithubIssueId",
                table: "Issues",
                column: "GithubIssueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchIssues_IssueId",
                table: "SearchIssues",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchIssues_SearchId_IssueId",
                table: "SearchIssues",
                columns: new[] { "SearchId", "IssueId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchIssues");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Searches");
        }
    }
}
