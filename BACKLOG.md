# TermBullet Execution Backlog

This file tracks the V1 implementation order, current execution status, and
remaining work.

## Current Snapshot

Complete:

- repository scaffold;
- smoke test setup;
- Domain model;
- public refs and generation;
- Application item lifecycle use cases;
- monthly JSON path resolver;
- safe JSON writer with backup/recovery;
- JSON item repository;
- root-level history for create/update/delete;
- local JSON index rebuild and automatic update after writes;
- local data path reporting;
- first-run data directory selection persisted in install-directory `conf.json`;
- delete and clear-history use cases;
- data path use case;
- CLI command flows for item lifecycle, collections, search, path, and history
  clear;
- simplified in-place migration between collections;
- repository/schema conformance for optional fields;
- TUI MVP with Main Dashboard, Search, Add Item, Notes, Calendar, Tags,
  contextual help, focus updates, search state, add flow, and dashboard
  lifecycle shortcuts;
- initial TUI Item Detail screen with available item fields;
- initial TUI Migrate Item flow with basic item data and destination preview;
- TUI Planning placeholder for future AI-assisted planning;
- TUI Week, Backlog, and Forgotten review views;
- task, note, and event specific Add Item flows;
- quick task capture for today's tasks;
- official task status model aligned to `open`, `done`, and `cancelled`;
- manual TUI smoke validation;
- baseline documentation.

Still missing for V1 hardening:

- optional release hardening beyond the current offline core.

## V1 Goal

Deliver a local-first offline terminal planner with CLI, TUI MVP, tasks, notes,
events, Today, Week, Backlog, Forgotten review, monthly JSON persistence, local
JSON index, search, editing, migration, movement, and data path discovery.

## Milestone Status

| Milestone | Status |
| --- | --- |
| 0 - Repository Scaffold | Complete |
| 1 - Domain | Complete |
| 2 - Application Use Cases | Complete |
| 3 - JSON Repositories | Complete |
| 4 - CLI MVP | Complete |
| 5 - Data Path and History | Complete |
| 6 - TUI MVP | Complete |
| 7 - V1 Release Candidate | Partial |

Milestone responsibilities:

- **0 Scaffold:** solution, projects, smoke test, build/test pipeline.
- **1 Domain:** item model, status/priority/collection rules, public refs.
- **2 Application:** item lifecycle, collections, search, and data path use
  cases.
- **3 Repositories:** monthly JSON files, safe writes, backup/recovery, local
  index, data path reporting, and in-place migration history.
- **4 CLI:** official command tree from [CLI.md](CLI.md).
- **5 Data/path:** path and history clear.
- **6 TUI MVP:** Main Dashboard, Search, Add Item, Item Detail, Migrate Item,
  Planning placeholder, Week, Backlog, Forgotten, Notes, Calendar, and Tags
  using [screens.md](screens.md).
- **7 Release candidate:** validation, regression tests, smoke tests, release
  notes.

## Milestone 6 Notes

The active TUI MVP is intentionally limited to:

- Main Dashboard;
- Search;
- Item Detail;
- Migrate Item;
- Planning placeholder;
- Week View;
- Backlog;
- Forgotten;
- Notes;
- Calendar;
- Tags;
- Add Item auxiliary flow.

Deferred post-MVP screens:

- AI Planning implementation;
- Review;

The current TUI visual reference is [screens.md](screens.md).

## Milestone 7 Remaining

- [x] Implement validated Notes screen
- [x] Implement validated Calendar screen
- [x] Implement validated Tags and Create Tag flow
- [ ] Decide whether the CLI needs a derived `forgotten` command
- [x] Expose per-item JSON history through Application contracts for Item Detail
- [ ] JSON file backup/recovery validation
- [ ] Cross-platform smoke testing where practical
- [x] Release notes draft
- [ ] Regression tests for item lifecycle from create to done/cancel/migrate
- [ ] Regression tests for public ref sequence behavior
- [ ] Regression tests for CLI and Application consistency
- [ ] Regression tests for persistence round trip
- [x] Replace current-working-directory data storage with first-run data root
      configuration

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
- [x] Replace old task status model with `open`, `done`, and `cancelled`
- [x] Remove task planning dates and use task collections for Today, Week,
      Month, and Backlog
- [x] Refactor manual migration to require `--collection <today|week|month|backlog>`
- [x] Refactor migrate to move the same task between collections without
      duplicating items
- [x] Add TUI coverage for the Forgotten review flow

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
