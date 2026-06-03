# TermBullet CLI

The CLI is a first-class interface for capture, lookup, and manipulation without
opening the TUI. It must use System.CommandLine and call Application use cases.

When no command is provided, TermBullet opens the TUI.

```bash
termbullet
```

## Help Shape

```text
TermBullet - Local-First Terminal Planner

Usage:
  termbullet [command] [arguments] [options]

If no command is provided, the main TUI is opened.
```

## Command Tree

```text
termbullet
├── add
├── list
├── today
├── week
├── month
├── backlog
├── show
├── edit
├── done
├── cancel
├── migrate
├── move
├── delete
├── tag
├── untag
├── priority
├── search
├── ai
│   ├── chat
│   ├── plan
│   └── profile
├── history
│   └── clear
└── path
```

## Global Options

- `-h`, `--help`: show help.
- `-v`, `--version`: show version.

## Core Commands

### TUI

Open the TUI by running TermBullet without a command:

```bash
termbullet
```

There is no separate `tui` command in V1. The shortest command opens the main
terminal interface.

### `add`

Create an item.

```bash
termbullet add "fix jwt authentication"
termbullet add "error happens when audience is empty" --note
termbullet add "review 16:00" --event
```

Type flags are mutually exclusive:

- `--task`
- `--note`
- `--event`

Default type is task.

Task items use `today`, `week`, `month`, or `backlog`. Notes are saved in the
`notes` collection. Events are saved in the `events` collection.

### `list`

List items, with filters where supported.

```bash
termbullet list
```

### Views and Collection Shortcuts

```bash
termbullet today
termbullet week
termbullet month
termbullet backlog
```

`today`, `week`, `month`, and `backlog` show tasks in their respective
collections. They are not date-grouped task schedules. Notes and events are
available through `list`, `show`, `search`, and the TUI Notes/Calendar screens.

Forgotten is currently exposed in the TUI as a derived review list. There is no
active `forgotten` CLI command in the current command tree.

### `show`

Show one item by public ref.

```bash
termbullet show t-0426-1
```

### `edit`

Edit item content and optional description when supported.

```bash
termbullet edit t-0426-1 "fix auth flow"
```

### State Changes

```bash
termbullet done t-0426-1
termbullet cancel t-0426-1
termbullet migrate t-0426-1 --collection week
termbullet migrate t-0426-1 --collection backlog
```

`migrate` applies to tasks and must receive a destination collection:

- `--collection today`
- `--collection week`
- `--collection month`
- `--collection backlog`

Open tasks from previous monthly files that were not done or cancelled are
shown in the TUI Forgotten review for manual action.

### Movement

```bash
termbullet move t-0426-1 today
termbullet move t-0426-1 backlog
```

### Tags and Task Priority

```bash
termbullet tag t-0426-1 auth
termbullet untag t-0426-1 auth
termbullet priority t-0426-1 high
```

Each item has one tag. `tag` replaces the current item tag. `untag` resets the
item to the protected `default` tag when the provided tag matches the current
tag.

Priorities:

- `none`
- `low`
- `medium`
- `high`

Priority is task metadata. Notes and events are stored with `none`.

### `search`

Search items in local data.

```bash
termbullet search "jwt"
```

Search may read across all monthly JSON files. It is a lookup surface and does
not change item state.

### `delete`

Remove an active item and append a `deleted` history event with a snapshot.

```bash
termbullet delete t-0426-1
```

### `history clear`

Clear stored history entries, not active items.

```bash
termbullet history clear
```

### `path`

```bash
termbullet path
```

Show the active local config and data paths.

Example output:

```text
config: C:\Users\Alexsander\AppData\Local\TermBullet\bin\conf.json
data_root: C:\Users\Alexsander\Documents\TermBullet
data: C:\Users\Alexsander\Documents\TermBullet\data
```

On first execution, TermBullet asks for the base data directory, validates
read/write permissions, and saves the selection in `<install-dir>/conf.json`.
There are no user-editable product keys.

## Planned V2 AI Configuration Commands

AI connection configuration is CLI-only in V2 MVP. The TUI reads the selected
profile but does not edit AI provider settings.

### `ai profile add`

```bash
termbullet ai profile add local \
  --provider openai-compatible \
  --model llama3.1 \
  --base-url http://localhost:11434/v1 \
  --no-api-key
```

Adds or updates a named AI connection profile in `<install-dir>/conf.json`.
Environment variables are the preferred API key source.

For local models, Ollama is the recommended setup. Its default
OpenAI-compatible base URL is:

```text
http://localhost:11434/v1
```

Hosted provider example:

```bash
termbullet ai profile add cloud \
  --provider openai-compatible \
  --model gpt-4.1-mini \
  --base-url https://api.openai.com/v1 \
  --api-key-env TERMBULLET_OPENAI_API_KEY
```

### `ai profile list`

```bash
termbullet ai profile list
```

Shows registered profile names, provider, model, and active status. It must not
print API keys.

Example output:

```text
* local  openai-compatible   llama3.1
  cloud  openai-compatible   gpt-4.1-mini
```

### `ai profile use`

```bash
termbullet ai profile use local
```

Sets the active AI profile used by Planning.

### `ai profile show`

```bash
termbullet ai profile show local
```

Shows one profile without exposing secret values.

### `ai profile test`

```bash
termbullet ai profile test local
```

Validates provider, model, base URL, key source, and provider communication with
a short chat-completions request. A failed test returns a clear actionable error.

### `ai profile remove`

```bash
termbullet ai profile remove local
```

Removes a profile. Removing the active profile must require selecting another
active profile or leaving AI unconfigured.

## V2 AI Planning Commands

### `ai plan`

```bash
termbullet ai plan new-project --prompt "Plan the billing module"
termbullet ai plan new-weekly --prompt "Organize my week"
termbullet ai plan revise-weekly --prompt "Suggest next steps"
termbullet ai plan revise-project --tag auth --prompt "Suggest next steps"
```

Generates a structured AI planning draft preview using the active AI profile.
By default, the command does not persist changes.

To apply the generated draft, explicit confirmation is required:

```bash
termbullet ai plan new-weekly --prompt "Organize my week" --apply --yes
termbullet ai plan new-weekly --prompt "Organize my week" --apply
```

When `--apply` is used without `--yes`, the CLI prompts for confirmation before
persisting the draft.

Allowed mode values:

- `new-project`
- `new-weekly`
- `revise-weekly`
- `revise-project`

Rules:

- `revise-project` requires `--tag`.
- The active AI profile is loaded from `<install-dir>/conf.json`.
- The planning agent prompt is loaded from
  `<install-dir>/agents/planning-bulletjournal-agent.md`.
- Provider output must parse as a valid structured draft before it is printed.
- The command mode is authoritative; provider drafts are validated against that
  requested mode before they can be applied.
- AI requests include a response envelope JSON template generated by the
  pipeline; the model is expected to fill that template instead of inventing a
  new response shape.
- Invalid provider output returns a clear error and does not change data.
- `--apply` requires either `--yes` or an interactive `yes` confirmation.
- Applying a draft uses the same Application use cases as manual item and tag
  creation.

Example output:

```text
model: llama3.1
mode: new_project
summary: Billing module first version.
actions:
1. create_tag
   name: billing
2. create_note
   tag: billing
   content: Billing module scope
3. create_task
   tag: billing
   collection: today
   priority: high
   content: Define invoice states
applied:
1. create_tag tag=billing
2. create_note n-0626-1 tag=billing collection=notes
3. create_task t-0626-1 tag=billing collection=today
```

## V2 AI Chat Commands

### `ai chat`

```bash
termbullet ai chat
```

Starts an interactive planning chat in the terminal using the active AI profile.
The chat is a CLI alternative to the TUI Planning screen and follows the same
draft-before-apply rule. The assistant may ask questions or discuss the plan in
normal text before returning a structured draft.

Optional arguments:

```bash
termbullet ai chat --profile cloud --mode new-project
termbullet ai chat --profile local --mode new-weekly
termbullet ai chat --mode revise-weekly
termbullet ai chat --mode revise-project --tag auth
```

Allowed `--mode` values:

- `new-project`
- `new-weekly`
- `revise-weekly`
- `revise-project`

Interactive commands:

```text
/mode new-project
/mode new-weekly
/mode revise-weekly
/mode revise-project auth
/apply
/discard
/exit
```

Rules:

- `ai chat` uses the active profile from `<install-dir>/conf.json`.
- `ai chat` must load `<install-dir>/agents/planning-bulletjournal-agent.md`
  before calling the model.
- AI output uses a response envelope JSON object. `draft_ready=false` displays
  chat text from `message`; `draft_ready=true` validates `draft` and shows a
  preview.
- AI output must be converted into a structured draft before it can change data.
- User messages may produce conversational assistant text or a validated draft
  preview.
- `ai chat` sends recent user and assistant turns with each new prompt so
  follow-up requests can refer to the current planning conversation.
- Prompts that explicitly ask to create, add, generate, or build tasks, plans,
  roadmaps, or drafts require a structured draft response.
- If a required draft response comes back as normal chat, TermBullet retries
  once with a repair instruction that asks the model to fill the draft template.
- `/apply` requires explicit confirmation before persisting changes.
- `/apply` can only persist the current validated draft.
- `/discard` drops the current draft without changing data.
- `revise-project` requires one selected non-default tag.
- `ai chat` must not expose API keys in logs, errors, or transcript output.
- if the planning agent prompt is missing or unreadable, `ai chat` must return a
  clear error and must not call the AI provider.

Example session:

```text
profile: local
mode: new-project

you> Plan the first version of the billing module.
ai> Tell me the expected outcome, constraints, deadline, and definition of done.
you> It must support invoices, payments, and failure states by the end of June.
ai> Draft ready: 1 tag, 1 scope note, 8 tasks.
you> /apply
confirm apply draft? yes
applied: 1 tag, 1 note, 8 tasks
```

## CLI Rules

- Keep command names and options aligned with this file.
- Keep help and output English-first.
- Keep behavior consistent with equivalent TUI actions.
- Do not implement business rules in command handlers.
- Verify parsing and representative help output when CLI behavior changes.
