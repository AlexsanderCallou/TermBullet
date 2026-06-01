# TermBullet Backlog

This file tracks the current V1 status and the next work candidates. Historical
implementation details belong in release notes and git history.

## Current Status

V1 offline core is complete and released.

Delivered:

- CLI and TUI over shared Application use cases.
- Tasks, notes, and events.
- Today, Week, Month, and Backlog task collections.
- Forgotten review as a derived TUI view.
- Create, list, show, edit, done, cancel, delete, migrate, move, tag, untag,
  priority, search, path, and history clear flows.
- First-run data root selection stored in install-directory `conf.json`.
- Monthly JSON persistence under `<data_root>/data`.
- Safe JSON writes, one backup per operational file, backup recovery, local
  JSON index, and readable JSON formatting.
- Tags catalog, item history, and Item Detail history display.
- Windows x64 and Linux x64 release assets.

## Open V1 Hardening

These are quality and distribution improvements, not blockers for the V1
offline core.

- Decide whether the CLI needs a derived `forgotten` command.
- Add broader regression tests for complete item lifecycle flows.
- Add broader persistence round-trip and backup/recovery tests.
- Run cross-platform smoke testing with published Windows and Linux binaries.
- Improve install, update, and uninstall workflows.

## Post-V1

### V2 - AI Planning

- BYOK provider/model/key/base URL setup.
- Planning profiles such as `plan-day`, `review-day`, `breakdown-task`, and
  `prioritize-backlog`.
- Preview-before-persisting workflow.
- Filtered AI context assembly from local JSON.

### V3 - Google Calendar

- Optional Google Calendar integration.
- Read daily calendar events.
- Show schedule context in the TUI.
- Create events from TermBullet when explicitly requested.

### V4 - Sync + Cloud

- Optional authentication and cloud sync.
- Push/pull synchronization.
- Whole-file monthly JSON synchronization.
- Conflict handling and sync history.
- Optional PostgreSQL backend storing the same JSON file content.

## Distribution

- Homebrew.
- Scoop.
- Winget.
- Chocolatey.
- Release automation.
