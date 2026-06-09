# TermBullet Release Notes

## v2.0.2 - TUI Standardization Patch

TermBullet V2.0.2 standardizes TUI screen behavior and removes text-input
shortcut conflicts.

- Added shared ASCII checkbox and radio formatting for TUI screens.
- Updated Planning, Search, Tags, Add/Edit Item, Migrate Item, Daily Review,
  Calendar, and collection screens to use consistent focus and action behavior.
- Protected text-input screens so ordinary letters and numbers do not trigger
  screen actions while typing.
- Renamed the visible main menu entry to `Daily`.
- Updated screen documentation, ADR guidance, and regression tests.

## v2.0.1 - Daily Review Patch

TermBullet V2.0.1 adds the manual Daily Review workflow for stale Today tasks
and aligns Today list behavior with local-day visibility.

- Added Daily Review in the TUI for stale open Today tasks.
- Added CLI commands under `daily` for review, keep, move, done, and cancel.
- Kept completed and cancelled Today tasks visible only on the local day when
  they reached a terminal status.
- Added `daily_reviewed` history events for keep-today decisions without
  changing `updated_at`.
- Updated documentation for the current V2 local-first and AI planning scope.

## v2.0.0 - Hybrid AI Profiles

TermBullet V2.0.0 improves AI provider configuration so direct-response models
and hosted reasoning models can coexist in the same `.aiconf` file.

### Added

- Hybrid AI profile behavior settings: `reasoning`, `test_max_tokens`,
  `chat_max_tokens`, and `planning_max_tokens`.
- Larger default token budgets for reasoning profiles.
- Recommended OpenCode Zen `deepseek-v4-flash-free` hosted reasoning profile
  examples.

### Changed

- `test-ai` now uses each profile's `test_max_tokens` instead of a fixed tiny
  output limit.
- Chat and planning requests now use per-profile token budgets unless a request
  explicitly overrides them.
- Empty provider responses caused by `finish_reason=length` now return a clearer
  configuration hint.

## v1.3.0 - Guided AI Planning

TermBullet V1.3.0 adds optional AI-assisted guided planning for creating new
project plans from structured prompts.

### Added

- AI profile configuration commands and OpenAI-compatible provider support.
- Guided Planning TUI for creating new project drafts from topic, project tag,
  task volume, and today-start choices.
- Structured AI planning draft generation, preview, approval, and apply
  workflow.
- Canonical planning agent prompt shipped with published binaries.
- README guidance for the initial AI setup.

### Changed

- Planning is scoped to new project creation only; existing-plan review is
  deferred for future work because broad historical review is not reliable
  enough yet.
- AI draft validation now accepts only create actions and repairs invalid
  collection placeholders once before failing.

## v1.2.0 - Tags and Monthly Carry-Over

TermBullet V1.2.0 makes tags a first-class planning surface and finalizes the
current alpha JSON shape for item tagging.

### Added

- Tags dashboard shortcut and searchable Tags screen.
- Tag Detail screen with tasks, notes, events, timeline, and tag-scoped create
  shortcuts.
- Monthly carry-over for open non-default tagged tasks and notes.

### Changed

- Items now store one `tag` instead of a `tags` array.
- Untagging returns an item to the protected `default` tag.
- Open `default` tasks from previous months stay in Forgotten for manual review.

## v1.1.2 - Offline Core Hardening

TermBullet V1.1.2 resolves documentation and runtime integrity gaps found
after the readable JSON release.

### Changed

- The CLI documentation now makes the no-command TUI startup explicit and no
  longer documents a separate `tui` command.
- `--help`, `-h`, `--version`, and `-v` can run without first creating
  `conf.json`.
- JSON history timestamps now use the injected application clock.
- Item Detail now reads and displays real per-item JSON history.

## v1.1.1 - Readable JSON Storage

TermBullet V1.1.1 keeps the first-run data directory setup and improves local
file readability.

### Added

- README uninstall instructions for safely removing the executable while
  preserving local data.

### Changed

- Monthly JSON files are now written with indentation.
- The local `index.json` file is now written with indentation.

## v1.1.0 - First-Run Data Directory Setup

TermBullet V1.1 keeps the offline core and adds first-run data directory setup.

### Added

- First-run prompt for choosing the local data root.
- Install-directory `conf.json` with the selected `data_root`.
- Startup validation for read/write permissions in the selected data directory.
- Clear startup error when the install directory cannot write `conf.json`.
- `termbullet path` output for config, data root, and operational data paths.

### Changed

- Operational data no longer depends on the current working directory.
- Monthly JSON, tags, and index files now live under `<data_root>/data`.
- Windows release publishing can be run with `publish.ps1`.

## v1.0.0 - V1 Offline Core

TermBullet V1 is the first offline core release. It provides a local-first
terminal planner for tasks, notes, events, and review workflows.

### Included

- Keyboard-first TUI with Main Dashboard, Search, Item Detail, Add Item, Migrate
  Item, Planning placeholder, Week, Month, Backlog, Forgotten, Notes, Calendar,
  and Tags screens.
- CLI for capture, listing, item detail, search, status changes, editing,
  priority, tags, movement, migration, data path discovery, and history clear.
- Task, note, and event item model with public refs such as `t-0526-1`,
  `n-0526-1`, and `e-0526-1`.
- Task collections: Today, Week, Month, and Backlog.
- Forgotten review as a derived view for unresolved tasks from previous monthly
  files.
- Events with `scheduled_at`.
- Local monthly JSON persistence, safe writes, one backup per monthly file,
  backup recovery, and local JSON index.
- Windows x64 and Linux x64 install scripts that resolve the latest GitHub
  release and verify SHA256 checksums.

### Not Included

- AI execution.
- Google Calendar integration.
- Machine sync or cloud accounts.
- PostgreSQL runtime dependency.
- Export/import commands.

