# TermBullet Execution Backlog

This file tracks the V1 implementation order, current execution status, and
remaining work.

## Current Snapshot

Complete:

- repository scaffold;
- smoke test setup;
- Core domain model;
- public refs and generation;
- Application item lifecycle use cases;
- monthly JSON path resolver;
- safe JSON writer with backup/recovery;
- JSON item repository;
- root-level history for create/update/delete;
- local JSON index rebuild and automatic update after writes;
- local data path reporting;
- delete and clear-history use cases;
- export/import use cases and infrastructure;
- data path use case;
- CLI command flows for item lifecycle, collections, search, path, export,
  import, and history clear;
- migration metadata persistence;
- repository/schema conformance for optional fields;
- TUI MVP with Main Dashboard, Search, Add Item, contextual help, focus updates,
  search state, add flow, and dashboard lifecycle shortcuts;
- initial TUI Item Detail screen with available item fields;
- initial TUI Migrate Item flow with basic item data and destination preview;
- official task status model aligned to `open`, `done`, `cancelled`, and
  `migrate`;
- manual TUI smoke validation;
- baseline documentation.

Still missing for the experimental MVP:

- align implementation with approved task planning model;
- V1 release candidate hardening.

## V1 Goal

Deliver a local-first offline terminal planner with CLI, TUI MVP, tasks, notes,
events, Today, Backlog, Forgotten, Week as a planning view, monthly JSON
persistence, local JSON index, search, editing, migration, movement, data path
discovery, export, and import.

## Milestone Status

| Milestone | Status |
| --- | --- |
| 0 - Repository Scaffold | Complete |
| 1 - Core Domain | Complete |
| 2 - Application Use Cases | Complete |
| 3 - JSON File Infrastructure | Complete |
| 4 - CLI MVP | Complete |
| 5 - Export, Import, Data Path | Complete |
| 6 - TUI MVP | Complete |
| 7 - V1 Release Candidate | Partial |

Milestone responsibilities:

- **0 Scaffold:** solution, projects, smoke test, build/test pipeline.
- **1 Core:** item model, status/priority/collection rules, public refs.
- **2 Application:** item lifecycle, collections, search, data path, export/import
  use cases.
- **3 Infrastructure:** monthly JSON files, safe writes, backup/recovery, local
  index, data path reporting, migration metadata.
- **4 CLI:** official command tree from [CLI.md](CLI.md).
- **5 Portability/path:** export, import, path, history clear.
- **6 TUI MVP:** Main Dashboard, Search, Add Item using [screens.md](screens.md).
- **7 Release candidate:** validation, regression tests, smoke tests, release
  notes.

## Milestone 6 Notes

The active TUI MVP is intentionally limited to:

- Main Dashboard;
- Search;
- Item Detail;
- Migrate Item;
- Add Item auxiliary flow.

Deferred post-MVP screens:

- Daily Focus;
- Weekly Planning;
- Backlog Triage;
- Forgotten Review;
- Review;

The current TUI visual reference is [screens.md](screens.md).

## Milestone 7 Remaining

- [ ] Add task `planned_for` persistence, indexing, export, and import support
- [ ] Add `forgotten` collection and startup/day-begin review for stale open
      tasks
- [ ] Refactor `week` from persisted collection to date-derived view
- [ ] Refactor manual migration to require `--date <yyyy-mm-dd>` or `--backlog`
- [ ] Add CLI/TUI coverage for the `forgotten` review flow
- [ ] Expose per-item JSON history through Application contracts for Item Detail
- [ ] Import/export validation
- [ ] JSON file backup/recovery validation
- [ ] Cross-platform smoke testing where practical
- [ ] Release notes draft
- [ ] Regression tests for item lifecycle from create to done/cancel/migrate
- [ ] Regression tests for public ref sequence behavior
- [ ] Regression tests for CLI and Application consistency
- [ ] Regression tests for persistence round trip
- [ ] Regression tests for export/import round trip

Already done for Milestone 7:

- [x] Documentation review
- [x] Command help review
- [x] Validate `dotnet restore`
- [x] Validate `dotnet build`
- [x] Validate `dotnet test`
- [x] Run CLI smoke tests
- [x] Run TUI smoke tests
- [x] Ensure README and docs reflect actual behavior
- [x] Add initial TUI Item Detail screen
- [x] Add initial TUI Migrate Item flow
- [x] Replace old task status model with `open`, `done`, `cancelled`, and
      `migrate`

## Manual TUI Smoke Checklist

- [x] Open the TUI with no command.
- [x] Confirm Main Dashboard renders.
- [x] Press `?` and confirm contextual help opens in English.
- [x] Press `Esc` and confirm help closes.
- [x] Press `c`, type `- Manual smoke task`, press `Enter`.
- [x] Confirm the TUI returns to dashboard.
- [x] Confirm the created task appears.
- [x] Confirm monthly JSON finalizes, not only temp file.
- [x] Open Search with `/` or menu.
- [x] Search for `Manual smoke task`.
- [x] Press `Esc` and confirm dashboard returns.
- [x] Press `q` and confirm clean exit.

## Post-V1 Backlog

V2 - AI Planning:

- [ ] AI setup module
- [ ] BYOK provider/model/key/base URL support
- [ ] internal profiles: `plan-day`, `review-day`, `breakdown-task`,
  `prioritize-backlog`
- [ ] preview-before-persisting workflow
- [ ] filtered AI context assembly from local JSON

V3 - Google Calendar:

- [ ] optional Google Calendar integration
- [ ] read daily events
- [ ] show calendar events in TUI
- [ ] use schedule context for AI planning
- [ ] create event from a local item

V4 - Sync + Cloud:

- [ ] authentication
- [ ] push/pull synchronization
- [ ] whole-file monthly JSON synchronization
- [ ] conflict handling
- [ ] sync history
- [ ] optional PostgreSQL backend storing JSON file content

Distribution and maintenance:

- [ ] Homebrew
- [ ] Scoop
- [ ] Winget
- [ ] Chocolatey
- [ ] release automation and packaging workflow
- [ ] cross-platform binary publishing
- [ ] ADRs for package manager ownership, release automation, and V4 conflict
  handling
