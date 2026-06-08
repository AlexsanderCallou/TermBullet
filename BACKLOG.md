# TermBullet Backlog

This file tracks the current execution focus. Historical implementation details
belong in release notes and git history.

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
- Optional AI configuration through `<data_root>/.aiconf`.
- OpenCode Zen with `deepseek-v4-flash-free` documented as the recommended AI
  setup.
- Windows x64 and Linux x64 release assets.

## Active V2 Focus

V2 should now focus on two product problems:

1. Task collection behavior, especially stale daily work.
2. A wider AI planning scope now that the recommended free model is stronger.

### 1. Task Collections and Daily Work

The current collection model treats `today` as a normal persisted task bucket.
That keeps unfinished work visible, but it also lets old daily tasks stay in
Today forever. Completed daily tasks can also make the execution surface feel
stale because the current view mixes current work with historical evidence.

#### Topic 1.1 - Today View Semantics

Problem:

- `today` is doing two jobs: current execution lane and historical collection.
- Done or cancelled tasks should remain persisted for history, but they should
  not make Today feel blocked or old.
- Open tasks left in Today after the day changes need a deliberate product rule,
  not accidental permanence.

Direction to evaluate:

- Keep the persisted collection value as `today` for compatibility.
- Treat Today as an execution view that shows open current work plus tasks
  completed or cancelled on the current local day.
- Remove done and cancelled Today tasks from the default Today list on the next
  local day while keeping them visible through detail/history/search.
- Add a clear optional way to inspect completed Today work when needed.

Decided:

- Done and cancelled Today tasks remain visible for the current local day.
- They leave the default Today view on the next local day.
- Older completed or cancelled Today work is available through Search, Item
  Detail, and History only.

#### Topic 1.2 - Daily Rollover and Stale Open Tasks

Problem:

- Open tasks can stay in `today` across multiple days without the user noticing
  that they are stale.
- Moving every stale task automatically would be surprising, but doing nothing
  leaves the daily lane clogged.
- The current model intentionally avoids task dates, so the solution must work
  through collection, status, history, and monthly files.

Direction to evaluate:

- Add a manual Daily Review step for open Today tasks whose latest Today
  placement or review happened before the current local date.
- Offer explicit choices: keep in Today, move to Week, move to Month, move to
  Backlog, mark done, or cancel.
- Keep automatic migration out of V2 daily rollover.
- Use history timestamps for stale detection instead of adding task due dates.
- Choosing keep in Today records a `daily_reviewed` history event only and does
  not change `updated_at`.
- Keep Daily Review separate from Forgotten because Forgotten scans older
  monthly JSON files and can become heavier.

Decided:

- Daily rollover is manual, like Bullet Journal migration.
- The new area name is `Daily Review`.
- Forgotten keeps its current monthly/archive review role.
- `keep today` writes history only.
- Daily Review is a dedicated TUI screen.
- CLI exposes `daily review`, `daily keep`, `daily move`, `daily done`, and
  `daily cancel`.

### 2. Broader AI Planning

The previous planning flow constrained the user with fixed task-size choices
and deterministic distribution. That was useful for weaker local models, but
the recommended OpenCode Zen `deepseek-v4-flash-free` profile can support a more
natural planning flow.

#### Topic 2.1 - AI-Decided Plan Size and Breakdown

Problem:

- Small/medium/large task volume is too rigid.
- The user often knows the topic and desired outcome, but not the right number
  of tasks.
- The current deterministic distribution can fight the model's ability to plan
  sensible milestones.

Direction to evaluate:

- Let AI choose the task count based on topic complexity and user intent.
- Keep guardrails instead of fixed choices: maximum task count, required project
  tag, allowed collections, and explicit user approval.
- Ask the model to include a short planning rationale in the readable preview,
  while the structured draft remains machine-validated.
- Keep numeric task prefixes so the user can follow the plan in order.

Open decisions:

- What is the default maximum task count for AI-decided plans: 20, 30, or 40?
- Should the user still be able to force a small/medium/large cap when desired?

#### Topic 2.2 - Planning With Existing Open Work

Problem:

- AI planning currently behaves mostly like a new-project generator.
- Users need planning that considers existing open tasks, notes, and tags.
- Sending all monthly JSON files is unsafe and noisy, but sending no context
  makes the assistant forget the real workload.

Direction to evaluate:

- Build a filtered open-work snapshot for AI: active tags, open tasks, recent
  notes, current collections, and stale Today candidates.
- Let AI propose a plan that may create tasks, move tasks between collections,
  mark stale tasks for cancellation, and summarize conflicts.
- Continue requiring structured drafts plus explicit approval before any write.
- Keep broad historical review as future scope; start with current open work.

Open decisions:

- Which context is allowed by default: current month only, current tag only, or
  all open non-completed work?
- Which mutation actions should be allowed first: create, move, cancel, or
  priority changes?

## Verification Before Next Release

- Run `dotnet restore`.
- Run `dotnet build`.
- Run `dotnet test`.
- Run manual TUI smoke tests for Today, stale task review, AI planning preview,
  draft approval, and AI unavailable states.
- Confirm non-AI local-first flows keep working without internet or accounts.

## Future

### Google Calendar

- Optional Google Calendar integration.
- Read daily calendar events.
- Show schedule context in the TUI.
- Create events from TermBullet when explicitly requested.

### Sync and Cloud

- Optional authentication and cloud sync.
- Push/pull synchronization.
- Whole-file monthly JSON synchronization.
- Conflict handling and sync history.
- Optional PostgreSQL backend storing the same JSON file content.

### Distribution

- Homebrew.
- Scoop.
- Winget.
- Chocolatey.
- Release automation.
