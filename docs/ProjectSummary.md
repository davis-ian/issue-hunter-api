# Project Summary & Architecture

## Mission
IssueHunter continually ingests "good first issues" from GitHub, filters out tickets that already have momentum, and surfaces the best open work via a self-hosted API with an eventual Vue 3 UI. Stretch goals include notification hooks and Docker-based deployment.

## Technology Baseline
- .NET 8 web host with ASP.NET Core + Swagger
- EF Core + SQLite for lightweight persistence
- Background worker for GitHub ingestion
- GitHub REST search API (anonymous for now)

## Runtime Components
1. **API Host (`IssueHunterApi`)** – Configures DI, Swagger, and ensures the SQLite database exists under `data/app.db`.
2. **GitHub Ingestion (`IssuePollingWorker`)** – Background service that periodically polls GitHub using `GitHubService`, filters out pull-request results, upserts issues, and links them to their originating searches.
3. **Data Layer (`AppDbContext` + models)** – EF Core context with `Issues`, `Searches`, and `SearchIssues` tables plus uniqueness constraints to avoid duplicates.
4. **HTTP Surface (`IssuesController`, `SearchController`)** – Minimal endpoints for listing stored issues and managing search definitions; these will back the future Vue 3 UI.

## Data Model Snapshot
| Entity | Purpose | Key Fields |
| --- | --- | --- |
| `Issue` | Canonical record per GitHub issue | `GithubIssueId`, `Repository`, `IssueNumber`, `Labels`, timestamps |
| `Search` | User-defined GitHub search/profile definition | `Name`, `Description`, labels, languages, `Query`, `IntervalMinutes`, `Priority`, `LastPolledAt`, `NextRunAfter` |
| `SearchIssue` | Junction table to track discovery context | `SearchId`, `IssueId`, `DiscoveredAt` |

## Current Behavior
- Worker now iterates enabled search profiles, respecting the `IssuePolling` configuration (enabled flag, interval, run-on-startup) instead of running unconditionally.
- Issues with `pull_request` metadata are ignored, but there is no auth token so rate limits are low.
- A `GitHubIssueQueryBuilder` composes the GitHub search string from profile labels/languages and enforces `state:open` plus `no:assignee` so results are unclaimed issues.
- Controllers return raw entities without pagination or filtering yet.

## Near-Term Priorities
1. **Search Profiles & Rotation** – Finalize profile CRUD/seeding plus scheduling updates so each profile sets `NextRunAfter` after polling and rotates predictably.
2. **Claimed-Issue Filtering** – Extend heuristics beyond `no:assignee`/“not a PR” (e.g., detect linked PRs or noisy conversations) so surfaced work is still available.
3. **Manual Poll & Profile APIs** – Expose endpoints to list active profiles, trigger polls on demand, and return shaped issue DTOs suitable for the upcoming Vue UI.
4. **Vue UI Foundations** – Start the frontend with filtering/sorting views once the API delivers curated, claim-free issues.

## Self-Hosting Path
1. Local dev: `dotnet run --project IssueHunterApi` initializes `data/app.db` and exposes Swagger on `http://localhost:5015`.
2. Configure GitHub personal-access token via future `appsettings`/environment variables before scaling up polling.
3. When backend configuration stabilizes (polling controls, search CRUD), introduce Docker packaging with a mounted SQLite volume.

## Future Extensions
- Vue 3 web UI with filtering, favorites, and search management.
- Notification channels (email/webhook/desktop) once curation quality improves.
- Enhanced heuristics (detect assignees, open PRs, stale conversations).
- Multi-database support and observability tooling for production-grade deployments.
