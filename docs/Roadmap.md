# IssueHunter Roadmap

Legend: [Done], [In Progress], [Planned]

## Phase 0 – Bootstrap ([Done])
- [Done] Scaffold .NET 8 API with Swagger + controllers.
- [Done] Add EF Core models for `Issue`, `Search`, `SearchIssue` using SQLite storage.
- [Done] Implement background worker that polls a sample GitHub query and persists deduped issues.

## Phase 1 – Polling Controls & Search Scheduling ([In Progress])
- [In Progress] Introduce `IssuePolling` configuration section (enable flag, interval, run-on-startup) and bind strongly typed options.
- [In Progress] Update `IssuePollingWorker` to honor the new configuration, defaulting to disabled in dev.
- [Planned] Track `LastPolledAt` and optional per-search intervals so polling runs only when due.
- [Planned] Expose a manual poll endpoint to trigger ingestion on demand while the worker is disabled.
- [Planned] Document the configuration defaults and recommended overrides for local vs. production use.

## Phase 2 – Surfacing & UX ([Planned])
- [Planned] Extend Issues API with pagination, filters, and DTO shaping for UI consumption.
- [Planned] Build Vue 3 frontend (Vite) to manage searches and explore curated issues.
- [Planned] Add saved views and snooze/favorite actions stored in the backend.
- [Planned] Evaluate need for authentication/authorization for multi-user setups.

## Phase 3 – Notifications & Deployment ([Planned])
- [Planned] Implement notification engine (email/webhook/desktop) with per-search rules.
- [Planned] Continuously re-check stored issues for new PRs/assignees and auto-hide stale ones.
- [Planned] Add health/metrics endpoints plus structured logging.
- [Planned] Package the stack in Docker (API + eventual UI) once configuration is environment-driven and stable.
- [Planned] Publish deployment + backup guidance for self-hosters.

## Stretch Goals
- [Planned] Leverage GitHub GraphQL for richer metadata (stars, reactions, comment counts).
- [Planned] Support alternative databases (Postgres/MySQL) for larger installs.
- [Planned] Shareable search templates or remote catalog syncing between instances.
