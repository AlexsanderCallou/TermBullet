# TermBullet Backlog

This file tracks the current execution focus. Historical implementation details
belong in release notes and git history.

## Current Status

The backlog is intentionally empty after the V2 publication.

TermBullet currently has:

- the offline local-first core;
- CLI and TUI over shared Application use cases;
- monthly JSON persistence, tag catalog, local index, and item history;
- task, note, and event workflows;
- Today, Week, Month, Backlog, Forgotten, Daily Review, Notes, Calendar, Tags,
  Item Detail, Search, Add Item, Edit Item, and Migrate Item surfaces;
- optional AI planning through `<data_root>/.aiconf`;
- structured AI drafts that require explicit approval before persistence;
- the canonical planning agent installed beside the executable.

## Completed Planning Area

The first real refactoring cycle standardized TUI screens and removed
text-input shortcut conflicts.

Concrete pain:

- screens use different visual conventions for actions, checkboxes, radio
  choices, and footers;
- some screens with text input still expose action shortcuts or document action
  shortcuts that could conflict with typing;
- repeated screen code makes it easy for future screens to drift.

Expected user-visible preservation:

- existing item workflows still work;
- existing CLI behavior is unchanged;
- local JSON data remains untouched;
- screens remain keyboard-first and dense.

Completed refactor plan:

1. Done - Define shared TUI primitives.
   - Add shared formatting helpers for ASCII checkbox and radio labels:
     `[ x ]`, `[   ]`, `( x )`, and `(   )`.
   - Add a shared action button row helper with a maximum of four buttons per
     line.
   - Add a common footer wording for text-input screens:
     `Tab focus  Enter activate focused button  Esc back  ? help`.
   - Rename the visible Daily menu entry from `Dashboard` to `Daily`.
     Internal class names can be renamed later if that reduces churn.

2. Done - Protect text-input screens.
   - Audit Quick Task, Add Task, Add Note, Add Event, Edit Task, Edit Note,
     Edit Event, Create Tag, Search, and Planning.
   - Ensure ordinary letter keys do not trigger actions when `TextField` or
     `TextView` is present.
   - Move actions to visible buttons or focused popups.

3. Done - Normalize full-form screens.
   - Refactor Add and Edit flows to use the shared controls and button row.
   - Keep Quick Task as the compact popup reference.
   - Ensure `Enter` submits only when `Save` is focused.

4. Done - Normalize list screens.
   - Standardize Week, Month, Backlog, Daily Review, Forgotten, Notes, and Tags
     as List + Preview + Actions screens.
   - Keep letter shortcuts only on screens without text input, or expose the
     same command as a visible action button.

5. Done - Normalize specialized screens.
   - Update Search to use Query + Results + Preview + Actions.
   - Update Planning to use Setup + Rules + Draft Preview + Actions with no
     letter-key actions.
   - Review Calendar, Item Detail, Tag Detail, and Migrate Item for consistent
     footers and ASCII controls.

6. Done - Add regression tests.
   - TUI shortcut policy tests for text-input screens.
   - Screen view-model tests for checkbox/radio formatting.
   - Representative screen tests for action button labels and footer wording.
   - Numbered panel navigation tests for screens without text input.
   - Tests proving number keys do not steal focus/input on screens with text
     fields.
   - Existing `dotnet test` must keep passing.

7. Done - Validate general screen behavior.
   - From Daily, open Today, Week, Month, Backlog, Daily Review, Forgotten,
     Notes, Calendar, Tags, Search, Planning, and Item Detail.
   - Confirm tasks open correctly from each task collection screen:
     Today/Daily items, Week, Month, Backlog, Daily Review, and Forgotten.
   - Confirm item detail opens the same public ref that was selected.
   - Confirm Tag Detail opens from Tags and lists the selected tag's current
     tasks, notes, and events in the correct panels.
   - Confirm tasks opened from Tag Detail keep their original collection and
     public ref.
   - Confirm `1`-`9` navigate numbered panels on screens without text input.
   - Confirm `1`-`9` type into focused text fields, or are ignored as commands,
     on screens with text input.
   - Confirm `Tab`, `Shift+Tab`, `Enter`, `Esc`, and `?` behave consistently
     across all screen families.
   - Confirm all action buttons perform the same Application use case behavior
     as the previous shortcuts.

8. Ready - Manual smoke checklist before merging.
   - Create one task in each collection and open it from its collection screen.
   - Create one non-default tag, assign it to at least one task, note, and
     event, then open Tag Detail.
   - Type text containing `e`, `s`, `t`, `g`, `d`, `x`, `n`, and numbers in all
     text-input screens.
   - Verify no unintended action fires while typing.
   - Verify Daily menu label appears as `Daily`, not `Dashboard`.

Boundaries:

- TUI owns layout, focus, and shortcut policy.
- Application use cases remain unchanged.
- Repositories and JSON contracts remain unchanged.
- Documentation changes must land with each screen refactor.

## Future Ideas

Future ideas stay outside the active backlog until they are selected for a
cycle:

- broader AI planning over existing open work;
- Google Calendar integration;
- sync and cloud;
- package manager distribution;
- release automation.
