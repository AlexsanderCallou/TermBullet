# TermBullet Screens

This document captures the TUI screens that are currently implemented in code.
It is intentionally editable: use it as a working sketchpad for refining the
product direction before changing the Terminal.Gui implementation.

Source checked:

- `src/TermBullet/Tui/Navigation/TuiScreen.cs`
- `src/TermBullet/Tui/TermBulletTuiApp.cs`
- `src/TermBullet/Tui/Screens/SearchScreen.cs`
- `src/TermBullet/Tui/Screens/AddItemScreen.cs`

Current implemented screens:

- Main Dashboard
- Search
- Add Item auxiliary flow

Planned but not currently implemented as TUI screens:

- Item Detail
- Migrate Item
- Daily Focus
- Weekly Planning
- Backlog Triage
- Forgotten Review
- Review
- Calendar View
- Sync / Cloud

## Screen 01 - Main Dashboard

Status: implemented.

Role: main operational dashboard loaded when the TUI starts.

Navigation:

- `/` opens Search.
- `c` opens Add Item.
- `Tab` and `Shift+Tab` move panel focus.
- `Enter` opens the selected item detail.
- `x` marks the selected day item as done.
- `z` cancels the selected item through the root shortcut mapper.
- `>` migrates the selected day item.
- `d` deletes the selected day item.
- `?` opens contextual help.
- `q` quits.

Refined target ASCII layout:

```text
+ TermBullet - Daily YYYY-MM-DD ------------------------------------------------------------+
| 1 Menu              | 2 Day Items                       | 3 Details                     |
| > Dashboard         | > [ ] t-0526-1 Fix auth flow      | ref: t-0526-1                 |
|   Search            |   (.) n-0526-1 Capture edge case  | type: task                    |
|   Planning          |   (o) e-0526-1 Review 16:00       | status: open                  |
|   Calendar          |                                   | priority: normal              |
|                     |                                   | collection: today             |
|                     |                                   | tags: auth, cli               |
|---------------------+-----------------------------------+-------------------------------|
| 4 Context           | 5 Content                                                         |
| context             | Fix auth flow                                                     |
| > today      3      |                                                                  |
|   week view  8      | Description:                                                     |
|   backlog    14     | - reproduce login failure                                        |
|   forgotten  2      | - check token audience                                           |
| tags                |                                                                  |
| > auth  cli  docs   |                                                                  |
+-------------------------------------------------------------------------------------------+
| / filter  c add  e edit  x done  z cancel  > migrate  d delete  Enter open  Tab focus     |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The code names the second panel `Day Items`, not `Daily Log`.
- This cleaner dashboard removes AI-facing language from the main surface. AI
  should appear later inside planning flows that propose new tasks, not as a
  permanent dashboard panel.
- `Details` keeps structured metadata compact and leaves the larger lower panel
  for the selected item's actual content.
- `Context` replaces `Projects / Tags` because projects are not a current V1
  entity. It can show collection counts, the Week planning view, and active
  tags. Week is a view derived from `planned_for`, not a persisted collection.
- `Content` is the main reading/editing surface for the selected item. It
  should show the item's `content` and optional `description`; tasks do not
  currently have an embedded notes collection in the JSON model.
- `Enter open` should open the selected item in the Item Detail screen. The
  current dashboard implementation does not fully support this yet.
- The footer includes `e edit`, but edit is not currently handled by the
  dashboard key handling code.

## Screen 02 - Search

Status: implemented.

Role: item search screen and early command-palette foundation.

Navigation:

- `/` represents search mode in the footer.
- Type a query and press `Enter` in the query field to search.
- `Tab` and `Shift+Tab` move focus between Results and Preview.
- `Esc` returns to the previous screen.
- `?` opens contextual help.
- `q` quits.

Current ASCII layout:

```text
+ TermBullet - Search - data:local - ai:off - sync:idle - mode:search ----------------------+
| query: jwt                                                                               |
|-------------------------------------------------------------------------------------------|
| 1 Results                                    | 2 Preview                                  |
| > [ ] t-0526-1 Fix auth flow                | ref: t-0526-1                              |
|   (.) n-0526-1 Empty audience note          | collection: today                          |
|   [ ] t-0526-4 Review token logic           | priority: high                             |
|                                              | status: open                               |
|                                              |                                            |
|                                              |                                            |
|                                              |                                            |
+-------------------------------------------------------------------------------------------+
| / search  Enter open  Ctrl+e edit  Ctrl+x done  Tab focus  ? help  Esc back              |
+-------------------------------------------------------------------------------------------+
```

Notes:

- Search currently renders two panels: Results and Preview.
- The query field sits above the panels.
- The preview currently shows ref, collection, priority, and status.
- `Enter open` should open the selected result in the Item Detail screen.
- The footer advertises `Ctrl+e edit` and `Ctrl+x done`; these are
  product-direction shortcuts and are not fully wired in the current screen.

## Flow 03 - Add Item

Status: implemented as an auxiliary flow, not as a `TuiScreen` enum value.

Role: keyboard-only quick capture flow opened from the Main Dashboard with `c`.

Navigation:

- `Enter` submits the input.
- `Esc` cancels and returns to the previous dashboard state.
- `?` opens Add Item help.
- `q` quits.

Accepted quick-capture prefixes:

- `-` creates a task.
- `.` creates a note.
- `o` creates an event.

Current ASCII layout:

```text
+ TermBullet - Add Item - target:today -----------------------------------------------------+
| Add                                                                                       |
|                                                                                           |
| Item: fix auth flow                                                                       |
|                                                                                           |
| Error:                                                                                    |
|                                                                                           |
|-------------------------------------------------------------------------------------------|
| Examples                                                                                  |
| Prefixes:                                                                                 |
|   - task                                                                                  |
|   . note                                                                                  |
|   o event                                                                                 |
|                                                                                           |
| Examples:                                                                                 |
|   - fix jwt authentication                                                                |
|   . error happens when audience is empty                                                  |
|   o review 16:00                                                                          |
+-------------------------------------------------------------------------------------------+
| Enter add  Esc cancel  ? help  q quit                                                     |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The target collection is currently `today` when opened from the Main
  Dashboard.
- The input is parsed by `QuickCaptureParser`.
- Validation or use case errors appear on the `Error:` line.

## Screen 04 - Item Detail

Status: planned.

Role: full read view for one selected item. This screen opens from Main
Dashboard, Search, Forgotten Review, Backlog Triage, and any future list where
an item can be selected.

Navigation:

- `Esc` returns to the previous screen.
- `e` edits the item.
- `x` marks a task as done.
- `z` cancels a task.
- `>` migrates a task.
- `d` deletes the item.
- `Tab` and `Shift+Tab` move focus between sections when needed.
- `?` opens contextual help.
- `q` quits.

Target ASCII layout:

```text
+ TermBullet - Item t-0526-1 --------------------------------------------------------------+
| Fix auth flow                                                        task / open          |
|-------------------------------------------------------------------------------------------|
| Identity                         | Planning                                                |
| ref: t-0526-1                   | collection: today                                      |
| id: 0f3a9d94-4df0-47f7-95c1...  | planned_for: 2026-05-09                               |
| type: task                      | due_at: -                                             |
| status: open                    | scheduled_at: -                                       |
| priority: high                  | estimate: -                                           |
| tags: auth, cli                 |                                                       |
| version: 3                      | Migration                                             |
| created: 2026-05-09T08:14:00Z   | from: -                                               |
| updated: 2026-05-09T10:31:00Z   | to: -                                                 |
| completed: -                    | migrated_at: -                                        |
| canceled: -                     |                                                       |
|-------------------------------------------------------------------------------------------|
| Content                                                                                   |
| fix auth flow                                                                             |
|                                                                                           |
| Description                                                                               |
| - reproduce login failure                                                                 |
| - check token audience                                                                    |
|-------------------------------------------------------------------------------------------|
| History                                                                                   |
| 2026-05-09T08:14:00Z  created    created in today                                         |
| 2026-05-09T09:02:00Z  edited     priority none -> high                                    |
| 2026-05-09T10:31:00Z  tagged     added auth                                               |
+-------------------------------------------------------------------------------------------+
| e edit  x done  z cancel  > migrate  d delete  Tab focus  ? help  Esc back  q quit       |
+-------------------------------------------------------------------------------------------+
```

Migrated item example:

```text
+ TermBullet - Item t-0526-4 --------------------------------------------------------------+
| Fix auth flow                                                        task / open          |
|-------------------------------------------------------------------------------------------|
| Identity                         | Migration                                             |
| ref: t-0526-4                   | migrated_from_ref: t-0526-1                           |
| id: a0f13256-499f-47bc-a623...  | migrated_from_id: 0f3a9d94-4df0-47f7-95c1...          |
| type: task                      | migrated_at: 2026-05-09T20:15:00Z                    |
| status: open                    | source status: migrated                               |
| collection: today               |                                                       |
| planned_for: 2026-05-12         |                                                       |
|-------------------------------------------------------------------------------------------|
| Content                                                                                   |
| fix auth flow                                                                             |
|-------------------------------------------------------------------------------------------|
| History                                                                                   |
| 2026-05-09T20:15:00Z  migrated_from  created from t-0526-1 for 2026-05-12                |
+-------------------------------------------------------------------------------------------+
| e edit  x done  z cancel  > migrate  d delete  ? help  Esc back  q quit                  |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The Item Detail screen must show every persisted field that exists for the
  selected item, including null/empty values where useful for debugging.
- The history section should include root-level history entries related to the
  item, including create, edit, done, canceled, migrated, forgotten, deleted
  snapshots when applicable, and history cleanup metadata when relevant.
- Migration relationships must be visible in both directions:
  `migrated_from_*` on the destination item and `migrated_to_*` on the source
  item.
- Long internal IDs may be truncated visually, but the full value should be
  copyable or visible through a focused row in the final implementation.
- For notes and events, task-only fields may be shown as `-` or omitted only if
  the screen remains clear and complete.

## Flow 05 - Migrate Item

Status: planned.

Role: focused confirmation flow for migrating one task. It should stay simple:
show the basic item data, ask for one destination, and confirm or cancel.

Entry points:

- `>` from Main Dashboard selected task.
- `>` from Item Detail.
- Future list screens where a task is selected.

Navigation:

- `Tab` and `Shift+Tab` move between destination controls.
- `Space` toggles destination choice.
- `Enter` confirms migration.
- `Esc` cancels and returns to the previous screen.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Migrate t-0526-1 -----------------------------------------------------------+
| Item                                                                                      |
| ref: t-0526-1                                                                             |
| content: Fix auth flow                                                                    |
| status: open                                                                              |
| collection: today                                                                         |
| planned_for: 2026-05-09                                                                   |
| priority: high                                                                            |
| tags: auth, cli                                                                           |
|-------------------------------------------------------------------------------------------|
| Destination                                                                               |
| (x) Date                                                                                  |
|     planned_for: 2026-05-12                                                               |
| ( ) Backlog                                                                               |
|                                                                                           |
| Result                                                                                    |
| original: t-0526-1 -> migrated                                                           |
| new task:  open at 2026-05-12                                                             |
+-------------------------------------------------------------------------------------------+
| Enter migrate  Tab focus  Space toggle  Esc cancel  ? help                               |
+-------------------------------------------------------------------------------------------+
```

Backlog destination example:

```text
+ TermBullet - Migrate t-0526-1 -----------------------------------------------------------+
| Item                                                                                      |
| ref: t-0526-1                                                                             |
| content: Fix auth flow                                                                    |
| status: open                                                                              |
| collection: today                                                                         |
| planned_for: 2026-05-09                                                                   |
|-------------------------------------------------------------------------------------------|
| Destination                                                                               |
| ( ) Date                                                                                  |
|     planned_for: -                                                                        |
| (x) Backlog                                                                               |
|                                                                                           |
| Result                                                                                    |
| original: t-0526-1 -> migrated                                                           |
| new task:  open in backlog                                                                |
+-------------------------------------------------------------------------------------------+
| Enter migrate  Tab focus  Space toggle  Esc cancel  ? help                               |
+-------------------------------------------------------------------------------------------+
```

Notes:

- This flow applies only to tasks.
- It must require exactly one destination: Date or Backlog.
- Date migration requires a valid `yyyy-mm-dd` date.
- Backlog migration must not require a date.
- The flow should not expose the full history; that belongs to Item Detail.
- On confirmation, the original task remains stored with status `migrated`, and
  the destination is a new `open` task linked by migration fields.

## Implementation Gap Notes

The product spec describes a broader V1 TUI with Item Detail, Migrate Item,
Daily Focus, Weekly Planning, Backlog Triage, Forgotten Review, Review, and
Search. The active codebase currently contains only `MainDashboard` and `Search`
in `TuiScreen`, plus the Add Item auxiliary flow. This file documents the
current implemented state so the next design pass can adjust the intended
screens before implementation.
