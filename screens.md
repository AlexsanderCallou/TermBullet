# TermBullet Screens

This document captures the TUI screens that are currently implemented in code.
It is intentionally editable: use it as a working sketchpad for refining the
product direction before changing the Terminal.Gui implementation.

Source checked:

- `src/TermBullet/Tui/Navigation/TuiScreen.cs`
- `src/TermBullet/Tui/TermBulletTuiApp.cs`
- `src/TermBullet/Tui/Screens/SearchScreen.cs`
- `src/TermBullet/Tui/Screens/AddItemScreen.cs`
- `src/TermBullet/Tui/Screens/PlanningScreen.cs`
- `src/TermBullet/Tui/Screens/WeekScreen.cs`
- `src/TermBullet/Tui/Screens/ItemDetailScreen.cs`
- `src/TermBullet/Tui/Screens/MigrateItemScreen.cs`

Current implemented screens:

- Main Dashboard
- Search
- Add Item auxiliary flow
- Item Detail
- Planning placeholder
- Week View
- Backlog
- Forgotten
- Migrate Item

Planned but not currently implemented as TUI screens:

- Daily Focus
- Notes
- Calendar
- Tags
- AI Planning
- Review
- Calendar View
- Sync / Cloud

## Screen 01 - Main Dashboard

Status: implemented.

Role: main operational dashboard loaded when the TUI starts.

Navigation:

- `/` opens Search.
- `c` opens the Add Item type picker.
- `n` opens Quick Task and creates a task planned for today with only content.
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
|   Backlog           |                                   | priority: normal              |
|   Forgotten         |                                   | collection: today             |
|   Notes             |                                   |                               |
|   Calendar          |                                   |                               |
|   Tags              |                                   |                               |
|                     |                                   | planned_for: 2026-05-09       |
|                     |                                   | tags: auth, cli               |
|---------------------+-----------------------------------+-------------------------------|
| 4 Context           | 5 Content                                                         |
| context             | Fix auth flow                                                     |
| > today      3      |                                                                  |
|   week view  8      | Description:                                                     |
|   backlog    14     | - reproduce login failure                                        |
|   forgotten  2      | - check token audience                                           |
| tags                |                                                                  |
| > auth  cli         |                                                                  |
+-------------------------------------------------------------------------------------------+
| / filter  c add  n quick task  e edit  x done  z cancel  > migrate  d delete  Enter open  |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The code names the second panel `Day Items`, not `Daily Log`.
- This cleaner dashboard removes AI-facing language from the main surface. AI
  should appear later inside planning flows that propose new tasks, not as a
  permanent dashboard panel.
- `Details` keeps structured metadata compact and leaves the larger lower panel
  for the selected item's actual content.
- `Context` shows collection counts, the Week planning view, and active tags.
  Week is a view derived from `planned_for`, not a persisted collection.
- `Planning` opens a future AI-assisted planning placeholder. It is not the
  Week View and is not part of the V1 execution workflow.
- `Tags` opens the catalog view where tags can be created, inspected, and later
  selected while editing or creating items.
- `Content` is the main reading/editing surface for the selected item. It
  should show the item's `content` and optional `description`; tasks do not
  currently have an embedded notes collection in the JSON model.
- `Enter open` opens the selected item in the Item Detail screen.
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

Status: implemented.

Role: keyboard-first creation flow opened from the Main Dashboard. The flow is
split because tasks, notes, and events collect different fields.

Entry points:

- `c` opens the Add Item type picker.
- `n` opens Quick Task, a minimal one-field popup for a task planned for today.

### Flow 03A - Add Type Picker

Role: small modal selector shown after pressing `c`.

Navigation:

- `CursorUp` and `CursorDown` move between item types.
- `t`, `n`, and `e` jump directly to Task, Note, and Event.
- `Enter` confirms the selected type and opens the matching form.
- `Esc` cancels and returns to the dashboard.

ASCII layout:

```text
+------------------------- Add Item -------------------------+
| What do you want to add?                                  |
|                                                           |
| > Task   executable work with planned_for                 |
|   Note   reference or context, no planned date            |
|   Event  scheduled appointment with scheduled_at          |
|                                                           |
| Enter choose  t task  n note  e event  Esc cancel         |
+-----------------------------------------------------------+
```

Notes:

- The picker does not create an item by itself.
- The selected type decides which form opens next.
- Type-specific forms should not show irrelevant fields.

### Flow 03B - Quick Task

Role: fastest possible capture for a task planned for today, opened with `n`
from the dashboard.

Navigation:

- Type content and press `Enter` to create the task.
- `Esc` cancels.

Request mapping:

- `type`: `task`
- `collection`: `today`
- `planned_for`: today's date
- `content`: typed value
- `description`: `null`
- `tags`: empty
- `scheduled_at`: `null`

ASCII layout:

```text
+------------------------ Quick Task -----------------------+
| Task: fix auth flow                                      |
|                                                           |
| planned_for: today                                       |
| Enter add  Esc cancel                                    |
+-----------------------------------------------------------+
```

Notes:

- This is intentionally not the full task form.
- Empty content is invalid and should show an inline error in the modal.

### Flow 03C - Add Task

Role: full task form for work that may need planning metadata.

Navigation:

- `Tab` and `Shift+Tab` move between fields.
- `CursorUp` and `CursorDown` change the timing choice.
- `Space` cycles the timing choice.
- `Enter` submits.
- `Esc` returns to the dashboard.

Fields:

- `Content` required.
- `Description` optional multiline context.
- `Timing` required: `Today`, `Future date`, or `Backlog`.
- `Planned for` visible and required only for `Future date`.
- `Tags` optional comma-separated labels.

Request mapping:

- `type`: `task`
- `collection`: `today`, `week`, or `backlog`
- `planned_for`: today for `Today`, selected date for `Future date`, `null` for `Backlog`
- `scheduled_at`: `null`

ASCII layout:

```text
+ TermBullet - Add Task ------------------------------------------------+
| Content                                                              |
| fix auth flow                                                        |
|                                                                      |
| Description                                                          |
| reproduce login failure                                              |
| check token audience                                                 |
|                                                                      |
| Timing                                                               |
| > Today        planned_for: today                                    |
|   Future date  planned_for: 2026-05-12                               |
|   Backlog      planned_for: -                                        |
|                                                                      |
| Tags                                                                 |
| auth, cli                                                            |
+----------------------------------------------------------------------+
| Status: task | today | planned_for: today | tags: auth, cli          |
| Enter add  Tab focus  CursorUp/CursorDown move  Esc cancel  ? help   |
+----------------------------------------------------------------------+
```

### Flow 03D - Add Note

Role: capture reference material or context that is not executable work.

Navigation:

- `Tab` and `Shift+Tab` move between fields.
- `Enter` submits when focus is outside the multiline description.
- `Esc` returns to the dashboard.

Fields:

- `Title` or short `Content` required.
- `Description` optional multiline body.
- `Tags` optional comma-separated labels.

Request mapping:

- `type`: `note`
- `collection`: `backlog`
- `planned_for`: `null`
- `scheduled_at`: `null`

ASCII layout:

```text
+ TermBullet - Add Note -----------------------------------------------+
| Title                                                                |
| investigate stacktrace                                               |
|                                                                      |
| Description                                                          |
| error happens when token audience is empty                           |
|                                                                      |
| Tags                                                                 |
| auth, incident                                                       |
+----------------------------------------------------------------------+
| Status: note | no planned date | tags: auth, incident                |
| Enter add  Tab focus  Esc cancel  ? help                             |
+----------------------------------------------------------------------+
```

### Flow 03E - Add Event

Role: capture a scheduled appointment or time marker.

Navigation:

- `Tab` and `Shift+Tab` move between fields.
- `Enter` submits.
- `Esc` returns to the dashboard.

Fields:

- `Title` or short `Content` required.
- `Scheduled for` required. Initial implementation may use `yyyy-mm-dd`; later
  versions can add time input when the TUI model supports it cleanly.
- `Description` optional multiline context.
- `Tags` optional comma-separated labels.

Request mapping:

- `type`: `event`
- `collection`: `week`
- `planned_for`: `null`
- `scheduled_at`: selected scheduled date/time

ASCII layout:

```text
+ TermBullet - Add Event ----------------------------------------------+
| Title                                                                |
| dentist appointment                                                  |
|                                                                      |
| Scheduled for                                                        |
| 2026-05-12                                                           |
|                                                                      |
| Description                                                          |
| bring insurance card                                                 |
|                                                                      |
| Tags                                                                 |
| health                                                               |
+----------------------------------------------------------------------+
| Status: event | scheduled_at: 2026-05-12                             |
| Enter add  Tab focus  Esc cancel  ? help                             |
+----------------------------------------------------------------------+
```

## Screen 04 - Item Detail

Status: implemented.

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
| type: task                      | scheduled_at: -                                       |
| status: open                    |                                                       |
| priority: high                  |                                                       |
| tags: auth, cli                 |                                                       |
| version: 3                      | Migration                                             |
| created: 2026-05-09T08:14:00Z   | from: -                                               |
| updated: 2026-05-09T10:31:00Z   | to: -                                                 |
| completed: -                    | migrated_at: -                                        |
| cancelled: -                    |                                                       |
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

Migration destination item example:

```text
+ TermBullet - Item t-0526-4 --------------------------------------------------------------+
| Fix auth flow                                                        task / open          |
|-------------------------------------------------------------------------------------------|
| Identity                         | Migration                                             |
| ref: t-0526-4                   | migrated_from_ref: t-0526-1                           |
| id: a0f13256-499f-47bc-a623...  | migrated_from_id: 0f3a9d94-4df0-47f7-95c1...          |
| type: task                      | migrated_at: 2026-05-09T20:15:00Z                    |
| status: open                    | source status: migrate                                |
| collection: today               |                                                       |
| planned_for: 2026-05-12         |                                                       |
|-------------------------------------------------------------------------------------------|
| Content                                                                                   |
| fix auth flow                                                                             |
|-------------------------------------------------------------------------------------------|
| History                                                                                   |
| 2026-05-09T20:15:00Z  migrate_from   created from t-0526-1 for 2026-05-12                |
+-------------------------------------------------------------------------------------------+
| e edit  x done  z cancel  > migrate  d delete  ? help  Esc back  q quit                  |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The initial implementation shows all item fields currently exposed to the TUI.
- The history section currently explains that per-item history is not loaded by
  the existing Application contracts.
- The final implementation should include root-level history entries related to
  the item, including create, edit, done, cancelled, migrate, forgotten, deleted
  snapshots when applicable, and history cleanup metadata when relevant.
- Migration relationships must be visible in both directions:
  `migrated_from_*` on the destination item and `migrated_to_*` on the source
  item.
- Long internal IDs may be truncated visually, but the full value should be
  copyable or visible through a focused row in the final implementation.
- For notes and events, task-only fields may be shown as `-` or omitted only if
  the screen remains clear and complete.

## Screen 05 - Planning

Status: implemented placeholder.

Role: future AI-assisted planning workspace. Planning is where the user will
eventually ask TermBullet to help turn goals, backlog context, notes, and dated
work into proposed tasks.

This screen is not part of the V1 execution workflow. In V1 it must stay empty
and must not call AI, persist suggestions, or mutate items.

Entry points:

- `Enter` on `Planning` from the Main Dashboard menu.

Navigation:

- `Esc` returns to the dashboard.
- `?` opens contextual help.
- `q` quits.

Target ASCII layout:

```text
+ TermBullet - Planning ------------------------------------------------------------------+
| Future AI Planning                                                                       |
|                                                                                          |
| Planning will become the AI-assisted workspace for turning goals into tasks.              |
|                                                                                          |
| For now, this screen is intentionally empty. V1 keeps planning manual and local-first.    |
|                                                                                          |
| Future scope: goals, context selection, task suggestions, and preview before saving.      |
+------------------------------------------------------------------------------------------+
| ? help  Esc back  q quit                                                                 |
+------------------------------------------------------------------------------------------+
```

Notes:

- Planning is a future V2 surface, not a synonym for Week View.
- Planning must not create, edit, migrate, or delete items in V1.
- Future AI behavior must preview suggestions before saving them.
- Future AI behavior must operate on filtered local context, not the whole data
  set by default.

## Screen 06 - Week View

Status: implemented.

Role: planning view for tasks and events scheduled across the current week. Week
is derived from `planned_for` for tasks and `scheduled_at` for events; it is not
a separate persisted collection.

Entry points:

- Future shortcut: `w` from the dashboard.
- Future menu entry if Week View becomes a top-level dashboard route again.

Navigation:

- `CursorUp` and `CursorDown` move within the focused day list.
- `Tab` and `Shift+Tab` move focus between days and Preview.
- `Enter` opens Item Detail for the selected item.
- `>` migrates a selected task.
- `x` marks a selected task done.
- `z` cancels a selected task or event.
- `d` deletes the selected item.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Week 2026-05-11..2026-05-17 ---------------------------------------------+
| 1 Mon 05-11              | 2 Tue 05-12              | 3 Wed 05-13                    |
| > [ ] t-0526-4 Fix auth  |   (o) e-0526-1 Dentist   |   [ ] t-0526-9 Review import   |
|   [ ] t-0526-5 Tests     |   [ ] t-0526-7 Release   |                                |
|--------------------------+--------------------------+--------------------------------|
| 4 Thu 05-14              | 5 Fri 05-15              | 6 Weekend                      |
|   [ ] t-0526-8 Write doc |   (o) e-0526-2 Demo      |   [ ] t-0526-10 Weekly review  |
|                          |                          |                                |
|-----------------------------------------------------------------------------------------|
| 7 Preview                                                                               |
| ref: t-0526-4  type: task  status: open  planned_for: 2026-05-11                        |
| Fix auth flow                                                                           |
| Description: reproduce login failure                                                    |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Only tasks with `planned_for` in the visible week and events with
  `scheduled_at` in the visible week appear here.
- Backlog tasks do not appear until migrated to a date.
- Notes do not appear unless a future product decision gives notes a planning
  relation.
- Moving an item between days should use the same business rule as migration:
  the original task can be migrated when appropriate instead of silently editing
  history.

## Screen 07 - Backlog

Status: implemented.

Role: triage view for open tasks without `planned_for` and notes kept as
reference material.

Entry points:

- `Enter` on `Backlog` from the Main Dashboard menu.
- Future shortcut: `b` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move through backlog rows.
- `Tab` and `Shift+Tab` move focus between Backlog, Preview, and Actions.
- `Enter` opens Item Detail.
- `>` migrates a selected task to Today, Future date, or another backlog copy.
- `x` marks a selected task done.
- `z` cancels a selected task.
- `d` deletes the selected item.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Backlog ------------------------------------------------------------------+
| 1 Backlog                                        | 2 Preview                           |
| > [ ] t-0526-12 Refactor settings store         | ref: t-0526-12                      |
|   [ ] t-0526-13 Review CLI help                 | type: task                          |
|   (.) n-0526-2  OAuth notes                     | status: open                        |
|   (.) n-0526-3  Terminal.Gui research           | planned_for: -                      |
|                                                  | collection: backlog                 |
|                                                  | tags: infra, tui                    |
|--------------------------------------------------+-------------------------------------|
| 3 Actions                                                                              |
| > plan today   planned_for: today                                                       |
|   plan date    planned_for: 2026-05-15                                                  |
|   open detail                                                                          |
|   delete                                                                                |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Backlog task rows have `planned_for: null`.
- Notes may live in Backlog because they are not planned work.
- The primary action is planning: move a task from Backlog into Today or a
  future date.
- Event rows should not normally appear here because events require
  `scheduled_at`.

## Screen 08 - Forgotten

Status: implemented.

Role: review view for open tasks that were planned for a past date and were not
completed, cancelled, migrated, or replanned.

Entry points:

- `Enter` on `Forgotten` from the Main Dashboard menu.
- Future shortcut: `f` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move through forgotten items.
- `Tab` and `Shift+Tab` move focus between Items, Preview, and Resolution.
- `Enter` opens Item Detail.
- `>` migrates a selected task to Today, Future date, or Backlog.
- `x` marks the selected task done.
- `z` cancels the selected task.
- `d` deletes the selected task.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Forgotten ---------------------------------------------------------------+
| 1 Items                                         | 2 Preview                            |
| > [ ] t-0526-3 Fix flaky test      missed 3d   | ref: t-0526-3                       |
|   [ ] t-0526-6 Update docs         missed 1d   | type: task                          |
|   [ ] t-0526-8 Check backup path   missed 5d   | status: open                        |
|                                                  | planned_for: 2026-05-06             |
|                                                  | collection: today                   |
|                                                  | tags: tests                         |
|--------------------------------------------------+-------------------------------------|
| 3 Resolution                                                                           |
| > migrate to today      new planned_for: today                                         |
|   migrate to date       new planned_for: 2026-05-15                                    |
|   move to backlog       new planned_for: -                                             |
|   mark done                                                                            |
|   cancel                                                                               |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Forgotten is a derived review list, not a persisted collection.
- A task is forgotten when `status: open`, `planned_for` is before today, and
  the task is not already migrated.
- Notes do not appear here because they have no `planned_for`.
- Events may need a later overdue-events review, but this screen is task-first
  for V1.

## Screen 09 - Notes

Status: target design pending validation.

Role: focused reading view for every item with `type: note`. Notes are
reference material and should not be mixed with executable work in this view.

Entry points:

- `Enter` on `Notes` from the Main Dashboard menu.
- Future shortcut: `N` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move through note rows.
- `Tab` and `Shift+Tab` move focus between Notes, Preview, and Actions.
- `Enter` opens Item Detail for the selected note.
- `d` deletes the selected note.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Notes --------------------------------------------------------------------+
| 1 Notes                                         | 2 Preview                            |
| > (.) n-0526-1 Capture edge case               | ref: n-0526-1                       |
|   (.) n-0526-2 OAuth notes                     | type: note                          |
|   (.) n-0526-3 Terminal.Gui research           | status: open                        |
|   (.) n-0526-4 Import caveats                  | collection: backlog                 |
|                                                  | planned_for: -                      |
|                                                  | scheduled_at: -                     |
|                                                  | tags: auth, tui                     |
|--------------------------------------------------+-------------------------------------|
| 3 Actions                                                                              |
| > open detail                                                                          |
|   delete                                                                                |
|                                                                                         |
+-----------------------------------------------------------------------------------------+
| Enter open  d delete  Tab focus  ? help  Esc back  q quit                               |
+-----------------------------------------------------------------------------------------+
```

Notes:

- This screen lists only notes, regardless of collection.
- Notes do not expose planning actions because they do not use `planned_for` or
  `scheduled_at`.
- A note can still be opened in Item Detail to inspect identity, content,
  description, tags, and timestamps.
- Deleting a note must use the same delete use case as other item types.

## Screen 10 - Calendar

Status: target design pending validation.

Role: month-style planning view for dated work and scheduled events. Calendar is
a derived view, not a persisted collection.

Entry points:

- `Enter` on `Calendar` from the Main Dashboard menu.
- Future shortcut: `k` from the dashboard.

Navigation:

- `CursorLeft` and `CursorRight` move the selected day.
- `CursorUp` and `CursorDown` move by week.
- `[` and `]` move to the previous or next month.
- `Tab` and `Shift+Tab` move focus between Month, Day Items, Preview, and
  Actions.
- `Enter` opens Item Detail for the selected item in the focused day.
- `>` migrates a selected task.
- `x` marks a selected task done.
- `z` cancels a selected task or event.
- `d` deletes the selected item.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Calendar May 2026 ---------------------------------------------------------+
| 1 Month                                                                                 |
| Mon          Tue          Wed          Thu          Fri          Sat          Sun         |
|              01           02           03           04           05           06          |
| 07           08           09*          10           11           12           13          |
| [2]          [ ]          [ ]          (1)          [1]          -            -           |
| 14           15           16           17           18           19           20          |
| -            (2)          [1]          -            [ ]          -            -           |
| 21           22           23           24           25           26           27          |
| -            -            (1)          [2]          -            -            -           |
| 28           29           30           31                                               |
| -            [1]          -            (1)                                              |
|-----------------------------------------------------------------------------------------|
| 2 Day Items                                    | 3 Preview                             |
| > [ ] t-0526-1 Fix auth flow                  | ref: t-0526-1                        |
|   (o) e-0526-1 Review 16:00                   | type: task                           |
|                                                | status: open                         |
|                                                | planned_for: 2026-05-09              |
|                                                | scheduled_at: -                      |
|-----------------------------------------------------------------------------------------|
| 4 Actions                                                                              |
| > open detail   migrate task   mark done   cancel   delete                             |
+-----------------------------------------------------------------------------------------+
| Arrows day  [/] month  Enter open  > migrate  x done  z cancel  d delete  Esc back      |
+-----------------------------------------------------------------------------------------+
```

Legend:

- `[n]` means `n` tasks planned for that date.
- `(n)` means `n` events scheduled for that date.
- `*` marks today.
- A cell can show both task and event counts when both exist.

Notes:

- Calendar includes tasks with `planned_for` and events with `scheduled_at`.
- Notes do not appear because they have no calendar relation.
- Backlog tasks do not appear until they receive a `planned_for` date.
- Calendar must not convert tasks into events. Task and event remain distinct
  item types and keep their own fields.
- Moving a task to a different date should use the migration rule where
  appropriate; moving an event should be treated as an edit/reschedule behavior
  when that workflow exists.

## Screen 11 - Tags

Status: target design pending validation.

Role: catalog view for tags used by item metadata and by the dashboard Context
panel. Tags describe topics, areas, or grouping labels. This screen is not an
item list and must not create tasks, notes, or events by itself.

Entry points:

- `Enter` on `Tags` from the Main Dashboard menu.
- Future shortcut: `g` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move inside the focused list.
- `Tab` and `Shift+Tab` move focus between Tags, Preview, and Actions.
- `c` opens the Create Tag flow.
- `Enter` opens the selected tag preview.
- `d` deletes or removes the selected tag according to the final business rule.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Tags ----------------------------------------------------------------------+
| 1 Tags                                         | 2 Preview                            |
| > auth                                6 items | name: auth                           |
|   cli                                 4 items | usage: 6 items                       |
|   tui                                 3 items | active tasks: 3                      |
|   import                              2 items | notes: 2                             |
|   backup                              1 item  | events: 1                            |
|                                                 | last used: 2026-05-09                |
|                                                 |                                      |
|-------------------------------------------------+--------------------------------------|
| 3 Actions                                                                              |
| > create tag                                                                           |
|   rename selected                                                                      |
|   remove selected from all items                                                       |
+-----------------------------------------------------------------------------------------+
| c create  Enter preview  d delete  Tab focus  ? help  Esc back  q quit                 |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Tags are metadata strings attached to items; they are not item types.
- The current model has `tags` on items and no separate `project` field.
- The dashboard Context panel should show the most relevant active tags based on
  item usage.
- Deleting a tag needs a clear business rule before implementation: block
  deletion while referenced, or remove it from all items after confirmation.

## Flow 12 - Create Tag

Status: target design pending validation.

Role: compact creation flow opened from Tags. The user gives the tag a name and
can optionally add a short description if the final model supports tag catalog
metadata.

Entry points:

- `c` from Tags.
- `create tag` action from Tags.

Navigation:

- `Tab` and `Shift+Tab` move between Name, Description, and Preview.
- `Enter` creates the tag.
- `Esc` cancels and returns to Tags.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Create Tag ----------------------------------------------------------------+
| Name                                                                                    |
| auth                                                                                    |
|                                                                                         |
| Description                                                                             |
| authentication and authorization work                                                   |
|                                                                                         |
| Preview                                                                                 |
| name: auth                                                                              |
| description: authentication and authorization work                                      |
+-----------------------------------------------------------------------------------------+
| Enter create  Tab focus  Esc cancel  ? help                                             |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Empty name is invalid and should show an inline error.
- Names should be normalized consistently before persistence; the exact
  normalization rule belongs in the data model decision.
- Creating a tag catalog entry must not mutate existing items automatically.
- If tags become first-class catalog records instead of derived strings,
  documentation must update `DATA_MODEL.md` before implementation.

## Flow 13 - Migrate Item

Status: implemented.

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
| original: t-0526-1 -> migrate                                                            |
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
| original: t-0526-1 -> migrate                                                            |
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
- On confirmation, the original task remains stored with status `migrate`, and
  the destination is a new `open` task linked by migration fields.

## Implementation Gap Notes

The product spec describes a broader TUI with Daily Focus, AI Planning, Week,
Backlog Triage, Forgotten Review, Review, and Search. The active codebase
currently contains `MainDashboard`, `Search`, `ItemDetail`, `Planning`, `Week`,
`Backlog`, `Forgotten`, and `MigrateItem` in `TuiScreen`, plus the Add Item
auxiliary flow for type picking, quick task capture, and type-specific creation
forms. Notes, Calendar, and Tags are target designs pending validation before
implementation. Review remains a future screen outside the current route set.
