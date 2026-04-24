# TermBullet - Execution Backlog

This file is the operational backlog for TermBullet.

It is intentionally aligned with `DEVELOPMENT_PLAN.md`, but adapted to the current repository state so it can be used as the execution guide for the next implementation steps.

## Working Rules

- Follow TDD for every task.
- Write tests first.
- Cover valid and invalid scenarios with mocked or controlled data where applicable.
- Mark a task as complete only after all relevant tests pass.
- Keep V1 offline and local-first.
- Do not introduce V2, V3, or V4 features into V1 except for clear extension points.

## Current Repository Snapshot

Already implemented:

- [x] `TermBullet.sln`
- [x] `src/TermBullet/TermBullet.csproj`
- [x] `tests/TermBullet.Tests/TermBullet.Tests.csproj`
- [x] smoke test setup
- [x] Core domain model
- [x] public ref value object and generation
- [x] Application use cases for core item lifecycle
- [x] monthly JSON path resolver
- [x] safe JSON writer with backup/recovery
- [x] JSON file item repository
- [x] root-level history events for create/update/delete
- [x] local JSON index rebuild service
- [x] automatic local index rebuild after repository writes
- [x] local settings persistence
- [x] `DeleteItemUseCase`
- [x] `ClearHistoryUseCase`
- [x] export/import application use cases
- [x] configuration application use cases
- [x] JSON export/import infrastructure
- [x] scoped history maintenance service
- [x] bootstrap/runtime composition start
- [x] CLI module structure
- [x] CLI command flow for `config`, `export`, `import`, and `history clear`
- [x] CLI command flow for `add`, `list`, `show`, `today`, `week`, and `backlog`
- [x] CLI command flow for `edit`, `done`, `cancel`, `move`, `tag`, `untag`, `priority`, and `migrate`
- [x] startup-triggered month rollover
- [x] migration metadata persistence
- [x] repository/schema conformance for optional fields
- [x] project documentation baseline

Still missing to finish the experimental MVP:

- [ ] V1 release candidate hardening

## Standard Verification Commands

- [x] `dotnet restore`
- [x] `dotnet build`
- [x] `dotnet test`
- [x] `dotnet run --project src/TermBullet -- [command] [arguments] [options]`

## Milestone 0 - Repository Scaffold

Status: complete

- [x] Solution structure exists
- [x] Test framework is configured
- [x] Smoke test exists
- [x] Build and test run successfully

## Milestone 1 - Core Domain

Status: complete

- [x] Item entity
- [x] Item type enum
- [x] Item status rules
- [x] Priority rules
- [x] Collection rules
- [x] Public ref value object
- [x] Public ref generation policy/domain service
- [x] Core tests for valid and invalid scenarios

## Milestone 2 - Application Use Cases

Status: complete

Completed:

- [x] create item
- [x] list items
- [x] show item
- [x] edit item
- [x] mark done
- [x] cancel item
- [x] migrate item
- [x] move item
- [x] tag/untag item
- [x] set priority
- [x] search items
- [x] today/week/backlog queries
- [x] `DeleteItemUseCase`
- [x] `ClearHistoryUseCase`
- [x] export use case
- [x] import use case
- [x] config use cases: `config list`, `config get`, `config set`, `config path`

Remaining:

- [x] tests for delete success and not found
- [x] tests for history clear success and invalid scope
- [x] tests for config success and missing keys
- [x] tests for export/import success and failure

## Milestone 3 - JSON File Infrastructure

Status: complete

Completed:

- [x] monthly file path resolver
- [x] safe file writer with temporary file and atomic replacement
- [x] one-backup-per-month behavior
- [x] backup recovery for corrupted JSON
- [x] item repository
- [x] public ref sequence storage inside monthly files
- [x] local JSON index rebuild
- [x] physical delete with root-level history event
- [x] automatic local index rebuild after repository writes
- [x] local settings storage
- [x] settings file structure and persistence
- [x] settings path exposure
- [x] history cleanup at monthly-file level
- [x] export service for local monthly JSON data
- [x] import service with malformed JSON handling
- [x] import validation for duplicate public refs
- [x] import validation for duplicate internal IDs
- [x] export/import round-trip Infrastructure tests
- [x] month rollover support for open task migration
- [x] migration metadata persistence according to `DATA_MODEL.md`
- [x] migration-between-months Infrastructure tests
- [x] full repository/schema conformance for optional fields such as `migration`

Remaining:

- [x] tests for automatic index consistency after add/update/delete
- [x] tests for history cleanup with backup-safe behavior
- [x] settings persistence tests

## Milestone 4 - CLI MVP

Status: complete

- [x] Add System.CommandLine dependency
- [x] Create CLI module structure
- [x] Implement `termbullet add`
- [x] Implement `termbullet list`
- [x] Implement `termbullet today`
- [x] Implement `termbullet week`
- [x] Implement `termbullet backlog`
- [x] Implement `termbullet show`
- [x] Implement `termbullet edit`
- [x] Implement `termbullet done`
- [x] Implement `termbullet cancel`
- [x] Implement `termbullet migrate`
- [x] Implement `termbullet move`
- [x] Implement `termbullet delete`
- [x] Implement `termbullet tag`
- [x] Implement `termbullet untag`
- [x] Implement `termbullet priority`
- [x] Implement `termbullet search`
- [x] Implement global options where practical
  - [x] `-v, --version`: implemented in V1
  - post-MVP: `--data`, `--json`, `--no-color`, `--profile` (global)
- [x] Add CLI tests for valid parsing
- [x] Add CLI tests for missing required arguments
- [x] Add CLI tests for invalid options
- [x] Add CLI tests for mocked handler execution
- [x] Add CLI tests for success output
- [x] Add CLI smoke tests for core flows
- [x] Add CLI tests for error output
- [x] Add CLI tests for representative help output

## Milestone 5 - Export, Import, and Config

Status: complete

- [x] Implement `termbullet export`
- [x] Implement `termbullet import`
- [x] Implement `termbullet config list`
- [x] Implement `termbullet config get`
- [x] Implement `termbullet config set`
- [x] Implement `termbullet config path`
- [x] Implement `termbullet history clear`
- [x] Define JSON export format
- [x] Add tests for export empty data directory
- [x] Add tests for export populated data directory
- [x] Add tests for import valid data
- [x] Add tests for import malformed data
- [x] Add tests for import duplicate public refs
- [x] Validate exported data preserves IDs, refs, tags, status, collections, and timestamps
- [x] Add tests for config get/set
- [x] Add tests for missing config key
- [x] Add tests for history clear for one month
- [x] Add tests for history clear for all months

## Milestone 6 - TUI MVP

Status: complete

MVP scope adjustment: the active TUI code now keeps only Main Dashboard, Search, and the auxiliary Add Item flow. Daily Focus, Weekly Planning, Backlog Triage, Review, and Config are deferred until after this MVP is stable.

- [x] Add Terminal.Gui dependency (1.19.0)
- [x] Create TUI module structure (Tui/Navigation/, Tui/Screens/)
- [x] Start TUI when no command is provided
- [x] Implement Main Dashboard (shell + panels 1/2/3 + q to quit)
- [x] Add TUI tests for screen state initialization (TuiNavigationState)
- [x] Add TUI tests for focus movement (MoveNextPanel, MovePreviousPanel, wrap)
- [x] Add TUI tests for selected item changes (SelectNextDayItem, SelectPreviousDayItem)
- [x] Add TUI tests for view model mapping from use case results (symbols, priority, collection)
- [x] Add TUI tests for action dispatch (HandleDoneAsync, HandleCancelAsync, HandleDeleteAsync, HandleMigrateAsync)
- [x] Implement keyboard focus model (Tab/Shift+Tab wired in TermBulletTuiApp)
- [x] Implement footer shortcuts (x done, > migrate, d delete visible in footer)
- [x] Reduce active MVP TUI scope to Main Dashboard, Search, and Add Item
- [x] Remove deferred screen code and tests from the active MVP TUI path
- [x] Implement multi-screen navigation (RunLoop + NavigateTo + NavigateBack)
- [x] Add TUI tests for screen navigation (CanNavigateBack, NavigateTo, NavigateBack)
- [x] Implement Search screen
- [ ] Post-MVP: restore Daily Focus screen when needed
- [ ] Post-MVP: restore Backlog Triage screen when needed
- [ ] Post-MVP: restore Weekly Planning screen when needed
- [ ] Post-MVP: restore Review screen when needed
- [ ] Post-MVP: restore Config screen when needed
- [x] Add contextual help overlay (`?`) for active TUI screens and Add Item flow
- [x] Strengthen active panel styling with live title/focus updates
- [x] Refresh screen previews after in-screen selection changes
- [x] Run startup maintenance when opening the TUI
- [x] Add TUI tests for search query state
- [x] Refactor TUI runtime to use one Terminal.Gui session with replaceable screen roots
- [x] Reimplement TUI add flow (`c`) without freezing the terminal runtime
- [x] Wire dashboard task lifecycle shortcuts through global TUI key handling (`x` done, `z` cancel, `>` migrate, `d` delete)
- [x] Add manual TUI validation for add/create persistence and JSON write completion
- [x] Run manual terminal rendering smoke test

## Milestone 7 - V1 Release Candidate

Status: partial

- [x] Documentation review
- [x] Command help review
- [ ] Import/export validation
- [ ] JSON file backup/recovery validation
- [ ] Cross-platform smoke testing where practical
- [ ] Release notes draft
- [ ] Regression tests for item lifecycle from create to done/cancel/migrate
- [ ] Regression tests for public ref sequence behavior
- [ ] Regression tests for CLI and Application consistency
- [ ] Regression tests for persistence round trip
- [ ] Regression tests for export/import round trip
- [x] Validate `dotnet restore`
- [x] Validate `dotnet build`
- [x] Validate `dotnet test`
- [x] Run CLI smoke tests
- [x] Run TUI smoke tests
- [x] Ensure README and docs reflect actual behavior

## Definition of Done

A task is done only when:

- [ ] tests were written first
- [ ] valid and invalid mocked data were covered where applicable
- [ ] implementation is complete
- [ ] all relevant tests pass
- [ ] documentation is updated when behavior changes
- [ ] no V2/V3/V4 feature was accidentally introduced into V1 scope

## Immediate Next Slice

Recommended next execution slice, still aligned with the milestones above:

- [x] Milestone 4: global options decided — `-v/--version` implemented; `--data`, `--json`, `--no-color`, `--profile` (global) deferred to post-MVP
- [x] Milestone 6: TUI runtime refactored to avoid stop/restart navigation and root handler accumulation
- [x] Milestone 6: reimplement TUI add (`c`) with a non-modal, keyboard-only flow that never performs persistence inside the UI event handler
- [x] Manual smoke validation: `c -> Enter -> item persisted -> JSON file finalized`
- [x] Manual smoke validation: dashboard navigation, Search, `?`, `Esc`, and `q`
- [x] After manual smoke passes, mark Milestone 6 complete

## Manual TUI Smoke Checklist

Use a clean temporary working directory and run the built executable.

- [x] Open the TUI with no command.
- [x] Confirm the Main Dashboard renders.
- [x] Press `?` and confirm contextual help opens in English.
- [x] Press `Esc` and confirm help closes without leaving the TUI.
- [x] Press `c`, type `- Manual smoke task`, press `Enter`, and confirm the TUI returns to the dashboard.
- [x] Confirm the created task appears in the dashboard.
- [x] Confirm `data/<year>/data_<month>_<year>.json` is finalized, not left as only a temp file.
- [x] Press `/` or open Search from the dashboard.
- [x] Search for `Manual smoke task` and confirm the item appears.
- [x] Press `Esc` from Search and confirm it returns to the dashboard.
- [x] Press `q` and confirm the TUI exits cleanly.

## Post-V1 Backlog

These items are intentionally outside V1, but remain part of the project roadmap.

### V2 - AI Planning

- [ ] AI configuration module
- [ ] BYOK provider/model/key/base URL support
- [ ] internal profiles such as `plan-day`, `review-day`, `breakdown-task`, and `prioritize-backlog`
- [ ] preview-before-persisting workflow for AI suggestions
- [ ] filtered AI context assembly from local JSON data

### V3 - Google Calendar

- [ ] optional Google Calendar integration module
- [ ] reading daily events
- [ ] showing calendar events in the TUI
- [ ] using schedule context for AI planning
- [ ] creating an event from a local item

### V4 - Sync + Cloud

- [ ] authentication model
- [ ] push/pull synchronization
- [ ] whole-file monthly JSON synchronization
- [ ] conflict handling
- [ ] sync history
- [ ] optional cloud backend using PostgreSQL with JSON file content

### Deferred or Optional Improvements

- [ ] Homebrew distribution
- [ ] Scoop distribution
- [ ] Winget distribution
- [ ] Chocolatey distribution
- [ ] release automation and packaging workflow
- [ ] cross-platform binary publishing
- [ ] additional ADRs for package manager ownership, release automation, and V4 conflict handling
