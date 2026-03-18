# IssueHunter Roadmap

Legend: [Done], [In Progress], [Planned]

## Phase 0 – Bootstrap ([Done])
- [Done] Scaffold .NET 8 API with Swagger + controllers.
- [Done] Add EF Core models for `Issue`, `Search`, `SearchIssue` using SQLite storage.
- [Done] Implement background worker that polls a sample GitHub query and persists deduped issues.

## Phase 1 – Search Profiles & Polling Controls ([In Progress])
- [Done] Introduce `IssuePolling` configuration section (enable flag, interval, run-on-startup) and bind strongly typed options.
- [Done] Update `IssuePollingWorker` to honor the new configuration, defaulting to disabled in dev.
- [In Progress] Define reusable search profiles (labels, languages, cadence metadata) and seed initial configurations the worker can load without manual edits.
- [In Progress] Rotate through active profiles using `NextRunAfter` scheduling and the new GitHub query builder; still need to persist scheduling metadata after each poll.
- [In Progress] Implement first-pass heuristics that skip claimed issues (currently enforcing `no:assignee` and ignoring PR hits; next step is richer signal checks).

## Phase 2 – API Surface & Manual Poll ([In Progress])
- [Done] Expose Search profile read/create/update endpoints (`GET`, `GET by id`, `POST`, `PUT`) using DTO contracts.
- [Done] Add paginated issue listing endpoint for frontend/API testing.
- [In Progress] Expand issue responses to stable DTOs with filter inputs (`searchId`, label/language, viewed state).
- [Planned] Expose manual poll endpoint(s) for dev workflows.
- [Planned] Provide basic scoring/sorting parameters (freshness, label tiers) so the UI can prioritize results without extra logic.

## Phase 3 – Frontend, Notifications & Deployment ([In Progress])
- [In Progress] Bootstrap the Vue 3 frontend (Vite) for profile management and issue exploration.
- [Planned] Add saved views, favorites/snooze actions, and evaluate auth requirements for multi-user installs.
- [Planned] Implement notification engine (email/webhook/desktop) with per-profile rules.
- [Planned] Continuously re-check stored issues for new PRs/assignees and auto-hide stale ones.
- [Planned] Add health/metrics endpoints plus structured logging, then package everything in Docker with deployment/backups guidance.

## Stretch Goals
- [Planned] Leverage GitHub GraphQL for richer metadata (stars, reactions, comment counts).
- [Planned] Support alternative databases (Postgres/MySQL) for larger installs.
- [Planned] Shareable search templates or remote catalog syncing between instances.
