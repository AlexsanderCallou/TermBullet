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
- `src/TermBullet/Tui/Screens/ItemListScreen.cs`
- `src/TermBullet/Tui/Screens/CalendarScreen.cs`
- `src/TermBullet/Tui/Screens/TagsScreen.cs`
- `src/TermBullet/Tui/Screens/CreateTagScreen.cs`
- `src/TermBullet/Tui/Screens/ItemDetailScreen.cs`
- `src/TermBullet/Tui/Screens/MigrateItemScreen.cs`

Current implemented screens:

- Main Dashboard
- Search
- Add Item auxiliary flow
- Item Detail
- Planning placeholder
- Week View
- Month View
- Backlog
- Forgotten
- Notes
- Calendar
- Tags
- Create Tag auxiliary flow
- Migrate Item

Planned but not currently implemented as TUI screens:

- Review
- Sync / Cloud

## Layout Convention

Full-screen layouts must use the Main Dashboard top bar pattern:

```text
+ TermBullet - <Screen Title> ------------------------------------------------------------+
```

The top bar identifies only the product and current screen. Runtime state such
as storage mode, AI status, sync status, or internal mode must stay out of the
top bar unless a future screen explicitly needs that state in its own content
area. Auxiliary modal flows, such as Add Type Picker and Quick Task, keep their
compact modal title bars.

## Screen 01 - Main Dashboard

Status: implemented.

Role: main operational dashboard loaded when the TUI starts.

Navigation:

- `/` opens Search.
- `c` opens the Add Item type picker.
- `n` opens Quick Task and creates a task in Today with only content.
- `Tab`, `Shift+Tab`, or the visible panel number (`1`-`9`) move panel focus.
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
|   Month             |                                   | priority: normal              |
|   Backlog           |                                   | collection: today             |
|   Forgotten         |                                   |                               |
|   Notes             |                                   |                               |
|   Calendar          |                                   |                               |
|   Tags              |                                   |                               |
|                     |                                   | tag: auth                     |
|---------------------+-----------------------------------+-------------------------------|
| 4 Context           | 5 Content                                                         |
| context             | Fix auth flow                                                     |
| > today      3      |                                                                  |
|   week       8      | Description:                                                     |
|   month      5      | - reproduce login failure                                        |
|   backlog    14     | - check token audience                                           |
|   forgotten  2      |                                                                  |
| tags                |                                                                  |
| > auth  cli         |                                                                  |
+-------------------------------------------------------------------------------------------+
| / filter  c add  n quick task  e edit  x done  z cancel  > migrate  d delete  Enter open  |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The code names the second panel `Day Items`, not `Daily Log`.
- This cleaner dashboard removes AI-facing language from the main surface. AI
  should appear later inside the Planning workspace that proposes new tasks, not as a
  permanent dashboard panel.
- `Details` keeps structured metadata compact and leaves the larger lower panel
  for the selected item's actual content.
- `Context` shows collection counts for Today, Week, Month, Backlog, Forgotten,
  and active tags.
- `Planning` opens a future AI-assisted planning placeholder. It is not the
  Week View and is not part of the V1 execution workflow.
- `Tags` opens the catalog view where tags can be created, inspected, and later
  selected while editing or creating items.
- `Content` is the main reading/editing surface for the selected item. It
  should show the item's `content` and optional `description`; notes use their
  own persisted `notes` collection.
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

Target ASCII layout:

```text
+ TermBullet - Search -------------------------------------------------------------------+
| query: jwt                                                                               |
|-------------------------------------------------------------------------------------------|
| 1 Results                                    | 2 Preview                                  |
| > [ ] t-0526-1 Fix auth flow                | ref: t-0526-1                              |
|   (.) n-0526-1 Empty audience note          | type: task                                 |
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
- `n` opens Quick Task, a minimal one-field popup for a task in Today.

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
| > Task   executable work in a collection                  |
|   Note   reference or context, no schedule                |
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

Role: fastest possible capture for a task in Today, opened with `n`
from the dashboard.

Navigation:

- `Tab` and `Shift+Tab` move between Task, Save, and Cancel.
- `Enter` activates the focused control.
- `Save` creates the task.
- `Cancel` or `Esc` returns to the dashboard without creating anything.

Request mapping:

- `type`: `task`
- `collection`: `today`
- `content`: typed value
- `description`: `null`
- `tags`: empty
- `scheduled_at`: `null`

ASCII layout:

```text
+------------------------ Quick Task -----------------------+
| Task: fix auth flow                                      |
|                                                           |
| collection: today                                        |
|                                                           |
| [ Save ]  [ Cancel ]                                     |
| Enter activate  Tab focus  Esc cancel                    |
+-----------------------------------------------------------+
```

Notes:

- This is intentionally not the full task form.
- Empty content is invalid and should show an inline error in the modal.
- `Enter` must not create the task unless the focused control is `Save`.

### Flow 03C - Add Task

Role: full task form for work that needs a destination collection.

Navigation:

- `Tab` and `Shift+Tab` move between fields, choices, Save, and Cancel.
- `CursorUp` and `CursorDown` change the active timing or priority choice.
- `Space` cycles the active timing or priority choice.
- `Enter` activates the focused control.
- `Save` submits the form.
- `Cancel` or `Esc` returns to the dashboard without creating anything.

Fields:

- `Content` required.
- `Description` optional multiline context.
- `Timing` required: `Today`, `Week`, `Month`, or `Backlog`.
- `Priority` required: `None`, `Low`, `Medium`, or `High`.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: `task`
- `collection`: `today`, `week`, `month`, or `backlog`
- `priority`: selected priority
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
| > Today                                                            |
|   Week                                                             |
|   Month                                                            |
|   Backlog                                                          |
|                                                                      |
| Priority                                                             |
| > None   Low   Medium   High                                        |
|                                                                      |
| Tags                                                                 |
| > [x] auth                                                           |
|   [x] cli                                                            |
|   [ ] tui                                                            |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: task | today | priority: high                               |
| Enter activate  Tab focus  Arrows move  Space toggle  Esc cancel  ? help |
+----------------------------------------------------------------------+
```

Notes:

- `Enter` must not submit the form unless the focused control is `Save`.
- When focus is inside a multiline text field, `Enter` keeps its text-editing
  behavior.
- A tag is selected from catalog entries created in the Tags screen. The Add
  flow must not create new tag names from free text.

### Flow 03D - Add Note

Role: capture reference material or context that is not executable work.

Navigation:

- `Tab` and `Shift+Tab` move between fields, Save, and Cancel.
- `Enter` activates the focused control.
- `Save` submits the form.
- `Cancel` or `Esc` returns to the dashboard without creating anything.

Fields:

- `Title` or short `Content` required.
- `Description` optional multiline body.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: `note`
- `collection`: `backlog`
- `priority`: `none`
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
| > [x] auth                                                           |
|   [x] incident                                                       |
|   [ ] tui                                                            |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: note | tag: auth                                             |
| Enter activate  Tab focus  Esc cancel  ? help                        |
+----------------------------------------------------------------------+
```

Notes:

- `Enter` must not submit the form unless the focused control is `Save`.
- When focus is inside the multiline description, `Enter` keeps its text-editing
  behavior.
- A tag is selected from catalog entries created in the Tags screen. The Add
  flow must not create new tag names from free text.

### Flow 03E - Add Event

Role: capture a scheduled appointment or time marker.

Navigation:

- `Tab` and `Shift+Tab` move between fields, Save, and Cancel.
- `Enter` activates the focused control.
- `Save` submits the form.
- `Cancel` or `Esc` returns to the dashboard without creating anything.

Fields:

- `Title` or short `Content` required.
- `Scheduled for` required. Initial implementation may use `yyyy-mm-dd`; later
  versions can add time input when the TUI model supports it cleanly.
- `Description` optional multiline context.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: `event`
- `collection`: `week`
- `priority`: `none`
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
| > [x] health                                                         |
|   [ ] personal                                                       |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: event | scheduled_at: 2026-05-12                             |
| Enter activate  Tab focus  Esc cancel  ? help                        |
+----------------------------------------------------------------------+
```

Notes:

- `Enter` must not submit the form unless the focused control is `Save`.
- When focus is inside the multiline description, `Enter` keeps its text-editing
  behavior.
- A tag is selected from catalog entries created in the Tags screen. The Add
  flow must not create new tag names from free text.

## Flow 04 - Edit Item

Status: planned.

Role: edit an existing task, note, or event without changing its identity.

Entry points:

- `e` on the selected item from Main Dashboard.
- `e` on the selected item from Today, Week, Month, Backlog, Forgotten, Notes,
  Calendar, or Search.
- `e` from Item Detail.

Shared rules:

- Editing never changes `id`, `public_ref`, `type`, `created_at`, or current
  terminal timestamps.
- `Save` updates `updated_at`, increments `version`, persists the changed
  fields, and appends an `edited` history event.
- `Cancel` or `Esc` returns to the previous screen without saving changes.
- `Enter` activates the focused control.
- `Enter` must not save unless the focused control is `Save`.
- When focus is inside a multiline text field, `Enter` keeps its text-editing
  behavior.
- Validation errors appear inline above the footer and keep the user on the
  edit screen.

### Flow 04A - Edit Task

Role: edit executable work while keeping task-specific planning fields visible.

Navigation:

- `Tab` and `Shift+Tab` move between fields, choices, Save, and Cancel.
- `CursorUp` and `CursorDown` change the active collection or priority choice.
- `Space` cycles the active collection or priority choice.
- Number keys may move directly between panels/field groups.

Fields:

- `Content` required.
- `Description` optional multiline context.
- `Collection` required: `Today`, `Week`, `Month`, or `Backlog`.
- `Priority` required: `None`, `Low`, `Medium`, or `High`.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: unchanged, must remain `task`
- `content`: edited text
- `description`: edited multiline text or `null`
- `collection`: selected collection
- `priority`: selected priority
- `tags`: normalized labels
- `scheduled_at`: must remain `null`

ASCII layout:

```text
+ TermBullet - Edit Task t-0526-1 -------------------------------------+
| 1 Content                                                            |
| fix auth flow                                                        |
|                                                                      |
| 2 Description                                                        |
| reproduce login failure                                              |
| check token audience                                                 |
|                                                                      |
| 3 Collection                                                         |
| > Today                                                              |
|   Week                                                               |
|   Month                                                              |
|   Backlog                                                            |
|                                                                      |
| 4 Priority                                                           |
| > None   Low   Medium   High                                         |
|                                                                      |
| 5 Tags                                                               |
| > [x] auth                                                           |
|   [x] cli                                                            |
|   [ ] tui                                                            |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: task | ref: t-0526-1 | today | priority: high                |
| Enter activate  Tab/1-5 focus  Arrows move  Space toggle  Esc cancel |
+----------------------------------------------------------------------+
```

Notes:

- Changing `Collection` from this screen is a normal edit, not the `migrate`
  flow. The `migrate` flow remains the deliberate Bullet Journal action for
  moving a task between collections.
- Notes and events must not expose priority controls.
- A tag is selected from catalog entries created in the Tags screen. Edit Item
  must not create new tag names from free text.

### Flow 04B - Edit Note

Role: edit reference material or context that is not executable work.

Navigation:

- `Tab` and `Shift+Tab` move between fields, Save, and Cancel.
- Number keys may move directly between field groups.

Fields:

- `Title` or short `Content` required.
- `Description` optional multiline body.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: unchanged, must remain `note`
- `content`: edited title/content
- `description`: edited multiline text or `null`
- `collection`: unchanged
- `priority`: must remain `none`
- `scheduled_at`: must remain `null`

ASCII layout:

```text
+ TermBullet - Edit Note n-0526-1 -------------------------------------+
| 1 Title                                                              |
| investigate stacktrace                                               |
|                                                                      |
| 2 Description                                                        |
| error happens when token audience is empty                           |
| include terminal log and repro steps                                 |
|                                                                      |
| 3 Tags                                                               |
| > [x] auth                                                           |
|   [x] incident                                                       |
|   [ ] tui                                                            |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: note | ref: n-0526-1 | tag: auth                             |
| Enter activate  Tab/1-3 focus  Esc cancel  ? help                    |
+----------------------------------------------------------------------+
```

Notes:

- Note editing must not expose collection, priority, or scheduled date as
  primary planning fields.
- A tag is selected from catalog entries created in the Tags screen.

### Flow 04C - Edit Event

Role: edit a scheduled appointment or time marker.

Navigation:

- `Tab` and `Shift+Tab` move between fields, Save, and Cancel.
- Number keys may move directly between field groups.

Fields:

- `Title` or short `Content` required.
- `Scheduled for` required. Initial implementation may use `yyyy-mm-dd`; later
  versions can add time input when the TUI model supports it cleanly.
- `Description` optional multiline context.
- `Tags` optional selection from the existing tag catalog.

Request mapping:

- `type`: unchanged, must remain `event`
- `content`: edited title/content
- `description`: edited multiline text or `null`
- `collection`: unchanged
- `priority`: must remain `none`
- `scheduled_at`: selected scheduled date/time

ASCII layout:

```text
+ TermBullet - Edit Event e-0526-1 ------------------------------------+
| 1 Title                                                              |
| dentist appointment                                                  |
|                                                                      |
| 2 Scheduled for                                                      |
| 2026-05-12                                                           |
|                                                                      |
| 3 Description                                                        |
| bring insurance card                                                 |
|                                                                      |
| 4 Tags                                                               |
| > [x] health                                                         |
|   [ ] personal                                                       |
|                                                                      |
| [ Save ]  [ Cancel ]                                                 |
+----------------------------------------------------------------------+
| Status: event | ref: e-0526-1 | scheduled_at: 2026-05-12             |
| Enter activate  Tab/1-4 focus  Esc cancel  ? help                    |
+----------------------------------------------------------------------+
```

Notes:

- Event editing must require `scheduled_at`.
- Event editing must not expose task collection or priority as primary
  planning fields.
- A tag is selected from catalog entries created in the Tags screen.

## Screen 04 - Item Detail

Status: implemented.

Role: focused read view for one selected item. This screen opens from Main
Dashboard, Search, Forgotten Review, Backlog Triage, Notes, Calendar, and any
future list where an item can be selected.

Navigation:

- `Esc` returns to the previous screen.
- `e` edits the item through the same type-specific form used for creation,
  prefilled with the current values.
- `Tab` and `Shift+Tab` move focus between the three panels.
- `1`, `2`, and `3` focus Planning/Info/Schedule, History, and Content.
- `?` opens contextual help.
- `q` quits.

Task detail target layout:

```text
+ TermBullet - Task t-0526-1 --------------------------------------------------------------+
|-------------------------------------------------------------------------------------------|
| 1 Planning                                      | 2 History                                |
| status: open                                    | 2026-05-09T08:14:00Z created            |
| collection: today                               | 2026-05-09T09:02:00Z edited             |
| priority: high                                  | 2026-05-09T10:31:00Z tagged             |
| tag: auth                                       |                                          |
|-------------------------------------------------------------------------------------------|
| 3 Content                                                                                 |
| title: Fix auth flow                                                                      |
|                                                                                           |
| description:                                                                              |
| reproduce login failure                                                                   |
| check token audience                                                                      |
+-------------------------------------------------------------------------------------------+
| e edit  Tab/1-3 focus  ? help  Esc back  q quit                                           |
+-------------------------------------------------------------------------------------------+
```

Note detail target layout:

```text
+ TermBullet - Note n-0526-1 --------------------------------------------------------------+
|-------------------------------------------------------------------------------------------|
| 1 Info                                          | 2 History                                |
| status: open                                    | 2026-05-09T08:14:00Z created            |
| tag: auth                                       | 2026-05-09T10:31:00Z edited             |
| updated: 2026-05-09T10:31:00Z                   |                                          |
|-------------------------------------------------------------------------------------------|
| 3 Content                                                                                 |
| title: OAuth notes                                                                        |
|                                                                                           |
| description:                                                                              |
| token refresh edge case notes                                                             |
+-------------------------------------------------------------------------------------------+
| e edit  Tab/1-3 focus  ? help  Esc back  q quit                                           |
+-------------------------------------------------------------------------------------------+
```

Event detail target layout:

```text
+ TermBullet - Event e-0526-1 -------------------------------------------------------------+
|-------------------------------------------------------------------------------------------|
| 1 Schedule                                      | 2 History                                |
| status: open                                    | 2026-05-09T08:14:00Z created            |
| scheduled: 2026-05-12                           | 2026-05-09T10:31:00Z edited             |
| tag: health                                     |                                          |
|-------------------------------------------------------------------------------------------|
| 3 Content                                                                                 |
| title: Dentist appointment                                                                |
|                                                                                           |
| description:                                                                              |
| bring insurance card                                                                      |
+-------------------------------------------------------------------------------------------+
| e edit  Tab/1-3 focus  ? help  Esc back  q quit                                           |
+-------------------------------------------------------------------------------------------+
```

Notes:

- The public ref appears only in the screen title.
- Internal IDs are not shown to users.
- Task-only fields are shown only for tasks.
- Note details must not show the fixed `notes` collection.
- Event details show scheduling only when the event has a schedule.
- The top row consumes roughly 30% of the screen height. Content consumes the
  remaining space so long notes stay readable.
- Empty or irrelevant fields should be omitted instead of displayed as `-`.

## Screen 05 - Planning

Status: partially implemented for V2.

Role: AI-assisted planning workspace. Planning is where the user asks
TermBullet to turn a goal, project scope, or weekly intent into a validated
proposal that can create or reorganize local items after explicit approval.

Planning currently has one purpose: create a new guided project plan. The user
fills fixed choices, then TermBullet asks the configured AI planning provider
for a validated draft preview. The draft can be applied or discarded.

Entry points:

- `Enter` on `Planning` from the Main Dashboard menu.

Navigation:

- `Tab` and `Shift+Tab` move focus between numbered panels.
- `g` generates a structured draft.
- `s` cycles task volume.
- `t` toggles the first task in Today.
- `Enter` sends the prompt or activates the selected action.
- `Esc` returns to the dashboard.
- `?` opens contextual help.
- `q` quits.

Planning target ASCII layout:

```text
+ TermBullet - Planning ------------------------------------------------------------------+
| 1 Setup                                      | 2 Rules                                   |
| Topic        Rust programming               | Volume: Medium                            |
| Project tag  studies-rust                   | Target range: 10-20 tasks                 |
| s: cycle task volume                        | Target tasks: 15                          |
| t: toggle first task today                  | Start today: Yes                          |
| g: generate structured draft                | Today: 1                                  |
| All task titles start with 1., 2., 3.       | Week: max 5 (5)                           |
|                                             | Month: max 20 (9)                         |
|                                             | Backlog: remaining (0)                    |
|---------------------------------------------+-------------------------------------------|
| 3 Draft Preview                                                                         |
| system> generating medium plan for studies-rust...                                      |
| assistant> draft ready: 15 actions.                                                     |
| draft> 1. Install the Rust toolchain                                                    |
| draft> 2. Learn cargo project basics                                                    |
|                                                                                     v   |
|-----------------------------------------------------------------------------------------|
| 4 Actions                                                                               |
| Generate draft                                                                          |
| Apply plan                                                                              |
| Discard draft                                                                           |
+-----------------------------------------------------------------------------------------+
| g generate  s size  t today  a apply  d discard  Tab focus  ? help  Esc back  q quit    |
+-----------------------------------------------------------------------------------------+
```

New Planning guided inputs:

- `Topic` describes the planning subject.
- `Project tag` is applied to every generated task.
- `Volume` cycles through `small`, `medium`, and `large`.
- `Start today` controls whether the first task is placed in `today`.
- Small creates up to 10 tasks, medium creates 10 to 20 tasks, and large creates
  20 to 40 tasks.
- New Planning places the first task in `today` when enabled, then up to 5 tasks
  in `week`, up to 20 tasks in `month`, and any remaining tasks in `backlog`.
- Every generated task title must start with a growing numeric prefix such as
  `1.`, `2.`, `3.` so the user can follow the plan in order.
- Editing the generated draft is deferred for V2 MVP. If the draft is wrong,
  the user regenerates or discards it.
- AI provider settings are configured through CLI commands only in V2 MVP. This
  screen only shows the active profile and configuration errors.

Reviewing existing plans is a future idea, not part of the current Planning
implementation. It is intentionally deferred because the current Planning design
targets small local models, and those models do not handle broad historical
review reliably enough yet.

```text
|-----------------------------------------------+-----------------------------------------|
| assistant> Select a review scope.                                                       |
| user> Review the auth project and suggest the next execution steps.                     |
|                                                                                     v   |
|-----------------------------------------------------------------------------------------|
+-----------------------------------------------------------------------------------------+
| Enter send/open  Up/Down scroll  PgUp/PgDn page  Tab focus  ? help  Esc back  q quit    |
+-----------------------------------------------------------------------------------------+
```


AI notes:

- AI never writes directly to monthly JSON files.
- AI may answer conversationally while planning. When it produces a structured
  draft, the application validates it, and only an approved draft is applied
  through Application use cases.
- AI responses use one JSON envelope. `draft_ready=false` renders `message` as
  chat, and `draft_ready=true` renders the validated `draft` preview.
- Planning sends recent user and assistant turns with each prompt so follow-up
  messages can refer to the current conversation.
- If the user explicitly asks to create, add, generate, or build tasks, plans,
  roadmaps, or drafts, Planning requires a structured draft instead of another
  conversational reply.
- If the required draft is returned as normal chat, Planning retries once with a
  draft-repair instruction before showing an error.
- Long assistant messages wrap inside the conversation panel.
- Structured draft JSON is rendered as a user-facing preview, not shown as raw
  JSON conversation text.
- AI context must be filtered to the selected planning mode and must not send
  all monthly JSON files by default.

## Screen 06 - Week View

Status: implemented.

Role: list view for tasks in the `week` collection. Week is a collection, not a
dated task schedule.

Entry points:

- Future shortcut: `w` from the dashboard.
- Future menu entry if Week View becomes a top-level dashboard route again.

Navigation:

- `CursorUp` and `CursorDown` move through task rows.
- `Tab` and `Shift+Tab` move focus between Week, Preview, and Actions.
- `Enter` opens Item Detail for the selected item.
- `>` migrates a selected task.
- `x` marks a selected task done.
- `z` cancels a selected task or event.
- `d` deletes the selected item.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Week ---------------------------------------------------------------------+
| 1 Week                                          | 2 Preview                           |
| > [ ] t-0526-4 Fix auth flow                   | ref: t-0526-4                      |
|   [ ] t-0526-5 Write tests                     | type: task                         |
|   [ ] t-0526-7 Review parser                   | status: open                       |
|                                                 | collection: week                   |
|-------------------------------------------------+------------------------------------|
| 3 Actions                                                                              |
| > migrate selected task                                                                |
|   open detail                                                                          |
|   mark done                                                                            |
|   cancel                                                                               |
|   delete                                                                               |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Only tasks in the `week` collection appear here.
- Events do not appear here; they belong to Calendar through `scheduled_at`.
- Migrating a task changes the same item's collection and keeps the same ref.

## Screen 06B - Month View

Status: implemented.

Role: list view for tasks in the `month` collection. Month is a collection, not
a dated task schedule.

Entry points:

- `Enter` on `Month` from the Main Dashboard menu.

Navigation and layout match Screen 06 - Week View, with the first panel titled
`Month` and rows loaded from the `month` collection.

## Screen 07 - Backlog

Status: implemented.

Role: triage view for open tasks in Backlog.
material.

Entry points:

- `Enter` on `Backlog` from the Main Dashboard menu.
- Future shortcut: `b` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move through backlog rows.
- `Tab` and `Shift+Tab` move focus between Backlog, Preview, and Actions.
- `Enter` opens Item Detail.
- `>` migrates a selected task to Today, Week, Month, or Backlog.
- `x` marks a selected task done.
- `z` cancels a selected task.
- `d` deletes the selected item.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Backlog ------------------------------------------------------------------+
| 1 Backlog                                        | 2 Preview                           |
| > [ ] t-0526-12 Refactor tag selector           | ref: t-0526-12                      |
|   [ ] t-0526-13 Review CLI help                 | type: task                          |
|   [ ] t-0526-14 Prepare release notes           | status: open                        |
|   [ ] t-0526-15 Review installer flow           | collection: backlog                 |
|                                                  | tag: infra                         |
|--------------------------------------------------+-------------------------------------|
| 3 Actions                                                                              |
| > migrate to today                                                                      |
|   migrate to week                                                                       |
|   migrate to month                                                                      |
|   open detail                                                                          |
|   delete                                                                                |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Notes may live in Backlog because they are not planned work.
- The primary action is migrating a task from Backlog into Today, Week, or
  Month.
- Event rows should not normally appear here because events require
  `scheduled_at`.

## Screen 08 - Forgotten

Status: implemented.

Role: review view for open tasks from previous monthly files that were not
completed or cancelled.

Entry points:

- `Enter` on `Forgotten` from the Main Dashboard menu.
- Future shortcut: `f` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move through forgotten items.
- `Tab` and `Shift+Tab` move focus between Items, Preview, and Resolution.
- `Enter` opens Item Detail.
- `>` migrates a selected task to Today, Week, Month, or Backlog.
- `x` marks the selected task done.
- `z` cancels the selected task.
- `d` deletes the selected task.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Forgotten ---------------------------------------------------------------+
| 1 Items                                         | 2 Preview                            |
| > [ ] t-0426-3 Fix flaky test      previous month | ref: t-0426-3                    |
|   [ ] t-0426-6 Update docs         previous month | type: task                       |
|   [ ] t-0426-8 Check backup path   previous month | status: open                     |
|                                                  | collection: today                   |
|                                                  | tag: tests                         |
|--------------------------------------------------+-------------------------------------|
| 3 Resolution                                                                           |
| > migrate to today                                                                     |
|   migrate to week                                                                      |
|   migrate to month                                                                     |
|   migrate to backlog                                                                   |
|   mark done                                                                            |
|   cancel                                                                               |
+-----------------------------------------------------------------------------------------+
| Enter open  > migrate  x done  z cancel  d delete  Tab focus  ? help  Esc back          |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Forgotten is a derived review list, not a persisted collection.
- A task is forgotten when `status: open` and it belongs to a previous monthly
  file.
- Forgotten may read all monthly files so old unresolved tasks remain visible
  without being automatically moved during month rollover.
- Notes do not appear here because Forgotten is task-first for V1.
- Events may need a later overdue-events review, but this screen is task-first
  for V1.

## Screen 09 - Notes

Status: implemented.

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
|   (.) n-0526-4 Storage caveats                 | tag: auth                           |
|                                                  | updated: 2026-05-09                 |
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

- This screen lists notes from the `notes` collection.
- Notes do not expose date actions because they do not use `scheduled_at`.
- A note can still be opened in Item Detail to inspect identity, content,
  description, tag, and timestamps.
- Deleting a note must use the same delete use case as other item types.

## Screen 10 - Calendar

Status: implemented.

Role: month-style schedule view for scheduled events from the `events`
collection.

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
- `z` cancels a selected event.
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
| -            -            -            (1)          -            -            -           |
| 14           15           16           17           18           19           20          |
| -            (2)          -            -            -            -            -           |
| 21           22           23           24           25           26           27          |
| -            -            (1)          -            -            -            -           |
| 28           29           30           31                                               |
| -            -            -            (1)                                              |
|-----------------------------------------------------------------------------------------|
| 2 Day Items                                    | 3 Preview                             |
| > (o) e-0526-1 Review 16:00                   | ref: e-0526-1                        |
|                                                | type: event                          |
|                                                | status: open                         |
|                                                | scheduled_at: 2026-05-09             |
|-----------------------------------------------------------------------------------------|
| 4 Actions                                                                              |
| > open detail   cancel   delete                                                        |
+-----------------------------------------------------------------------------------------+
| Arrows day  [/] month  Enter open  z cancel  d delete  Tab focus  ? help  Esc back      |
+-----------------------------------------------------------------------------------------+
```

Legend:

- `(n)` means `n` events scheduled for that date.
- `*` marks today.

Notes:

- Calendar includes events with `scheduled_at`.
- Calendar uses the current monthly operational set. It must not surface old
  unresolved tasks from previous months; those belong in Forgotten.
- Notes do not appear because they have no calendar relation.
- Tasks do not appear here because tasks are organized by collection, not date.
- Calendar must not convert tasks into events. Task and event remain distinct
  item types and keep their own fields.

## Screen 11 - Tags

Status: implemented.

Role: catalog view for tags used by item metadata and by the dashboard Context
panel. Tags describe topics, areas, or grouping labels. This screen is not an
item list and must not create tasks, notes, or events by itself.

Entry points:

- `Enter` on `Tags` from the Main Dashboard menu.
- `t` from the dashboard.

Navigation:

- `CursorUp` and `CursorDown` move inside the focused list.
- `Tab` and `Shift+Tab` move focus between Search, Tags, Preview, and Actions.
- `1`, `2`, `3`, and `4` focus Search, Tags, Preview, and Actions.
- `n` opens the Create Tag flow.
- `Enter` opens the selected tag detail.
- `Esc` returns to the dashboard.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Tags ----------------------------------------------------------------------+
| 1 Search                                                                                |
| auth                                                                                    |
|-----------------------------------------------------------------------------------------|
| 2 Tags                                         | 3 Preview                            |
| > # auth                         6 items      | name: auth                           |
|   # cli                          4 items      | usage: 6 items                       |
|   # tui                          3 items      | active tasks: 3                      |
|   # storage                      2 items      | notes: 2                             |
|                                                 | events: 1                            |
|                                                 | last used: 2026-05-09                |
|-------------------------------------------------+--------------------------------------|
| 4 Actions                                                                              |
| > open detail                                                                          |
|   create tag                                                                           |
+-----------------------------------------------------------------------------------------+
| Enter detail  n new  Tab/1-4 focus  ? help  Esc back  q quit                           |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Tags are metadata strings attached to items; they are not item types.
- Each item has exactly one `tag`; missing tags use protected `default`.
- The dashboard Context panel shows at most four active non-default tags, oldest
  first.
- The dashboard Context panel should show the most relevant active tags based on
  item usage.
- Removing or editing tag catalog entries is deferred until a clear business
  rule exists. Until then, the TUI must not advertise edit/delete for tags.

## Screen 12 - Tag Detail

Status: implemented.

Role: planning support view for one selected tag. It exposes every current item
belonging to that tag, split by item type and task collection.

Entry points:

- `Enter` from the Tags screen.

Navigation:

- `Tab` and `Shift+Tab` move focus between Summary, Timeline, Tasks, Notes, and
  Events.
- `1` through `5` focus Summary, Timeline, Tasks, Notes, and Events.
- `Enter` opens the selected item detail.
- `c` opens Add Item with the current tag preselected.
- `n` opens Quick Task with the current tag preselected.
- `e` edits the selected item through the type-specific edit form.
- `Esc` returns to Tags.

Target ASCII layout:

```text
+ TermBullet - Tag auth ------------------------------------------------------------------+
| 1 Summary                                    | 2 Timeline                              |
| tag: auth                                    | 2026-05-09 t-0526-1 Fix auth flow       |
| tasks: 4                                     | 2026-05-09 n-0526-2 OAuth notes         |
| notes: 2                                     |                                         |
| events: 1                                    |                                         |
|----------------------------------------------+------------------------------------------|
| 3 Tasks                                      | 4 Notes                                  |
| today                                        | > (.) n-0526-2 OAuth notes              |
| > [ ] t-0526-1 Fix auth flow                |                                          |
| week                                         |------------------------------------------|
|   [ ] t-0526-4 Rotate keys                  | 5 Events                                 |
| month                                        |   (o) e-0526-1 Auth review              |
|   [ ] t-0526-8 Review login errors          |                                          |
| backlog                                      |                                          |
|   [ ] t-0526-9 Document auth setup          |                                          |
+-----------------------------------------------------------------------------------------+
| Enter detail  c create  n quick task  e edit item  Tab/1-5 focus  Esc back  q quit      |
+-----------------------------------------------------------------------------------------+
```

Notes:

- The screen shows tasks from Today, Week, Month, and Backlog, plus notes and
  events that use the selected tag.
- Create and quick-task shortcuts must pass the current tag into the creation
  form.
- The screen is scoped to current operational data. Global search remains the
  lookup surface for previous monthly JSON files.

## Flow 12 - Create Tag

Status: implemented.

Role: compact creation flow opened from Tags. The user gives the tag a name and
can optionally add a short description if the final model supports tag catalog
metadata.

Entry points:

- `c` from Tags.
- `create tag` action from Tags.

Navigation:

- `Tab` and `Shift+Tab` move between Name, Description, Preview, Save, and
  Cancel.
- `Enter` activates the focused control.
- `Save` creates the tag.
- `Cancel` or `Esc` returns to Tags without creating anything.
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
|                                                                                         |
| [ Save ]  [ Cancel ]                                                                    |
+-----------------------------------------------------------------------------------------+
| Enter activate  Tab focus  Esc cancel  ? help                                           |
+-----------------------------------------------------------------------------------------+
```

Notes:

- Empty name is invalid and should show an inline error.
- Names should be normalized consistently before persistence; the exact
  normalization rule belongs in the data model decision.
- Creating a tag catalog entry must not mutate existing items automatically.
- Tag catalog entries are persisted in `data/tags.json`.
- `Enter` must not create the tag unless the focused control is `Save`.

## Flow 13 - Migrate Item

Status: implemented.

Role: focused confirmation flow for migrating one task. It should stay simple:
show the basic item data, ask for one destination, and confirm or cancel.

Entry points:

- `>` from Main Dashboard selected task.
- Future list screens where a task is selected.

Navigation:

- `Tab` and `Shift+Tab` move between destination controls, Save, and Cancel.
- `Space` toggles destination choice.
- `Enter` activates the focused control.
- `Save` confirms migration.
- `Cancel` or `Esc` returns to the previous screen without migrating.
- `?` opens contextual help.

Target ASCII layout:

```text
+ TermBullet - Migrate t-0526-1 -----------------------------------------------------------+
| Item                                                                                      |
| ref: t-0526-1                                                                             |
| content: Fix auth flow                                                                    |
| status: open                                                                              |
| collection: today                                                                         |
| priority: high                                                                            |
| tag: auth                                                                                 |
|-------------------------------------------------------------------------------------------|
| Destination                                                                               |
| (x) Today                                                                                 |
| ( ) Week                                                                                  |
| ( ) Month                                                                                 |
| ( ) Backlog                                                                               |
|                                                                                           |
| Result                                                                                    |
| t-0526-1: today -> today                                                                 |
| same task, same ref                                                                       |
|                                                                                           |
| [ Save ]  [ Cancel ]                                                                      |
+-------------------------------------------------------------------------------------------+
| Enter activate  Tab focus  Space toggle  Esc cancel  ? help                              |
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
|-------------------------------------------------------------------------------------------|
| Destination                                                                               |
| ( ) Today                                                                                 |
| ( ) Week                                                                                  |
| ( ) Month                                                                                 |
| (x) Backlog                                                                               |
|                                                                                           |
| Result                                                                                    |
| t-0526-1: today -> backlog                                                               |
| same task, same ref                                                                       |
|                                                                                           |
| [ Save ]  [ Cancel ]                                                                      |
+-------------------------------------------------------------------------------------------+
| Enter activate  Tab focus  Space toggle  Esc cancel  ? help                              |
+-------------------------------------------------------------------------------------------+
```

Notes:

- This flow applies only to tasks.
- It must require exactly one destination collection: Today, Week, Month, or
  Backlog.
- Migration must not require or edit a task date.
- The flow should not expose the full history; that belongs to Item Detail.
- On confirmation, the same task remains `open`, keeps the same `id` and
  `public_ref`, and changes only its `collection` plus `updated_at`.
- `Enter` must not confirm migration unless the focused control is `Save`.

## Implementation Gap Notes

The product spec describes a broader TUI with AI Planning, Week,
Backlog Triage, Forgotten Review, Review, Notes, Calendar, Tags, and Search.
The active codebase currently contains `MainDashboard`, `Search`, `ItemDetail`,
`Planning`, `Week`, `Backlog`, `Forgotten`, `Notes`, `Calendar`, `Tags`, and
`MigrateItem` in `TuiScreen`, plus the Add Item auxiliary flow for type picking,
quick task capture, type-specific creation forms, and the Create Tag flow.
Review remains a future screen outside the current route set.
