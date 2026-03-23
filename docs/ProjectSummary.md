# Project Summary & Architecture

## Mission

IssueHunter continually ingests "good first issues" from GitHub, filters out tickets that already have momentum, and surfaces the best open work via a self-hosted API with an eventual Vue 3 UI. Stretch goals include notification hooks and Docker-based deployment.

## Technology Baseline

- .NET 10 web host with ASP.NET Core + Swagger
- EF Core + SQLite for lightweight persistence
- Background worker for GitHub ingestion
- GitHub REST search API (anonymous for now)

## Runtime Components

1. **API Host (`IssueHunterApi`)** – Configures DI, Swagger, and ensures the SQLite database exists under `data/app.db`.
2. **Polling Runtime (`IssuePollingWorker` + `IssuePollingOrchestrator`)** – Background scheduler and orchestrator that run due searches, persist schedule metadata, and isolate per-search failures.
3. **GitHub Ingestion (`GitHubIssueIngestionService` + `GitHubService`)** – Builds GitHub search queries, filters out PR hits, upserts issues, and links discovered issues to their source searches.
4. **Data Layer (`AppDbContext` + models)** – EF Core context with `Issues`, `Searches`, and `SearchIssues` tables plus uniqueness constraints to avoid duplicates.
5. **HTTP Surface (`IssuesController`, `SearchController`, `PollingController`)** – CRUD for search profiles, paginated issue listing, and manual poll triggers.

## Data Model Snapshot

| Entity        | Purpose                                       | Key Fields                                                                                                       |
| ------------- | --------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `Issue`       | Canonical record per GitHub issue             | `GithubIssueId`, `Repository`, `IssueNumber`, `Labels`, timestamps                                               |
| `Search`      | User-defined GitHub search/profile definition | `Name`, `Description`, labels, languages, `Query`, `IntervalMinutes`, `Priority`, `LastPolledAt`, `NextRunAfter` |
| `SearchIssue` | Junction table to track discovery context     | `SearchId`, `IssueId`, `DiscoveredAt`                                                                            |

## Current Behavior

- Worker iterates enabled, due search profiles and persists polling metadata after each search run (`LastPolledAt`, `NextRunAfter`, `LastResultCount`, `LastError`).
- Poll orchestration now supports manual triggers (`POST /api/polling/all`, `POST /api/searches/{id}/poll`) with summary DTO responses.
- Ingestion uses batched upsert/link behavior and single-save-per-search persistence through the orchestrator.
- `GitHubIssueQueryBuilder` composes `q` terms (`state:open`, `no:assignee`, labels/languages/raw query); sorting is applied via GitHub API URL parameters.
- Search APIs use DTO-based create/update/read contracts; issues endpoint supports pagination for frontend usage.

## Near-Term Priorities

1. **Search Profile Seeding** – Add default C#/JS profile seeds and tighten profile validation guidance.
2. **Claimed-Issue Filtering** – Extend heuristics beyond `no:assignee`/“not a PR” (e.g., linked PRs, issue activity, stale signal checks).
3. **Issue DTO + Filters** – Replace raw issue payloads with stable response DTOs and add API filters (`searchId`, label text, repository text).
4. **Frontend Poll Actions** – Wire "Sync Now" and per-profile poll actions to manual poll endpoints with user-visible feedback.
5. **Docker MVP Packaging** – Build a simple self-hosted Docker Compose flow with persisted SQLite volume and setup docs.

## Self-Hosting Path

1. Local dev: `dotnet run --project IssueHunterApi` initializes `data/app.db` and exposes Swagger on `http://localhost:5015`.
2. Configure GitHub personal-access token via `appsettings`/environment variables before scaling up polling.
3. Run frontend against `/api` in dev via Vite proxy and in production via same-origin reverse proxy routing.
4. Package frontend + API in Docker with mounted SQLite volume and documented backup/restore steps.

## Future Extensions

- Vue 3 web UI with filtering, favorites, and search management.
- Notification channels (email/webhook/desktop) once curation quality improves.
- Enhanced heuristics (detect assignees, open PRs, stale conversations).
- Multi-database support and observability tooling for production-grade deployments.
