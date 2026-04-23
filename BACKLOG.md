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

Still missing to finish V1:

- [ ] remaining Infrastructure items
- [ ] missing Application use cases
- [ ] remaining Bootstrap/runtime composition
- [ ] remaining CLI MVP
- [ ] export/import/config/history command flows
- [ ] TUI MVP
- [ ] V1 release candidate hardening

## Standard Verification Commands

- [ ] `dotnet restore`
- [ ] `dotnet build`
- [ ] `dotnet test`
- [ ] `dotnet run --project src/TermBullet -- [command] [arguments] [options]`

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

Status: partially complete

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

Status: partially complete

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

Status: partially complete

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
- [x] Implement `termbullet tag`
- [x] Implement `termbullet untag`
- [x] Implement `termbullet priority`
- [x] Implement `termbullet search`
- [ ] Implement global options where practical
- [x] Add CLI tests for valid parsing
- [x] Add CLI tests for missing required arguments
- [x] Add CLI tests for invalid options
- [x] Add CLI tests for mocked handler execution
- [x] Add CLI tests for success output
- [x] Add CLI smoke tests for core flows
- [ ] Add CLI tests for error output
- [ ] Add CLI tests for representative help output

## Milestone 5 - Export, Import, and Config

Status: partially complete

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

Status: not started

- [ ] Add Terminal.Gui dependency
- [ ] Create TUI module structure
- [ ] Start TUI when no command is provided
- [ ] Implement Main Dashboard
- [ ] Implement Daily Focus
- [ ] Implement Backlog Triage
- [ ] Implement Search
- [ ] Implement Config
- [ ] Implement keyboard focus model
- [ ] Implement footer shortcuts
- [ ] Implement action dispatch to Application use cases
- [ ] Add TUI tests for screen state initialization
- [ ] Add TUI tests for focus movement
- [ ] Add TUI tests for selected item changes
- [ ] Add TUI tests for action dispatch
- [ ] Add TUI tests for search query state
- [ ] Add TUI tests for view model mapping from use case results
- [ ] Run manual terminal rendering smoke test

## Milestone 7 - V1 Release Candidate

Status: not started

- [ ] Documentation review
- [ ] Command help review
- [ ] Import/export validation
- [ ] JSON file backup/recovery validation
- [ ] Cross-platform smoke testing where practical
- [ ] Release notes draft
- [ ] Regression tests for item lifecycle from create to done/cancel/migrate
- [ ] Regression tests for public ref sequence behavior
- [ ] Regression tests for CLI and Application consistency
- [ ] Regression tests for persistence round trip
- [ ] Regression tests for export/import round trip
- [ ] Validate `dotnet restore`
- [ ] Validate `dotnet build`
- [ ] Validate `dotnet test`
- [x] Run CLI smoke tests
- [ ] Run TUI smoke tests
- [ ] Ensure README and docs reflect actual behavior

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

- [ ] Milestone 4/5: expand CLI help and error output coverage

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
