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
├── daily
│   ├── review
│   ├── keep
│   ├── move
│   ├── done
│   └── cancel
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
├── test-ai
├── set-ai
├── ai
│   ├── chat
│   └── plan
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

`today` shows open Today tasks plus tasks completed or cancelled on the current
local day. Older completed or cancelled Today tasks remain available through
`search`, `show`, and history.

### `daily`

Manual Bullet Journal review for stale open Today tasks.

```bash
termbullet daily review
termbullet daily keep t-0426-1
termbullet daily move t-0426-1 --collection week
termbullet daily move t-0426-1 --collection month
termbullet daily move t-0426-1 --collection backlog
termbullet daily done t-0426-1
termbullet daily cancel t-0426-1
```

`daily review` lists open tasks still in `today` whose latest Today placement or
Daily Review event happened before the current local date.

`daily keep` appends `daily_reviewed` history only. It does not change
`updated_at`, `version`, or the item collection.

`daily move` supports only `week`, `month`, and `backlog` destinations. `daily
done` and `daily cancel` use the normal terminal-status behavior.

Forgotten is exposed in the TUI as a separate derived review list for open tasks
from previous monthly files.

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

## AI Configuration

AI connection settings live in the data root:

```text
<data_root>/.aiconf
```

The file is plain text. Lines starting with `#` are comments. Each profile starts
with `[profile-name]`, followed by `key=value` settings.

If `.aiconf` does not exist, run:

```bash
termbullet test-ai
```

TermBullet creates a commented template and exits with an instruction to edit it.

Recommended OpenCode Zen configuration:

```ini
[opencode-free]
provider=openai-compatible
model=deepseek-v4-flash-free
base_url=https://opencode.ai/zen/v1
api_key_env=OPENCODE_API_KEY
default=true
reasoning=true
timeout_seconds=240
```

Create an OpenCode Zen API key from:

```text
https://opencode.ai/docs/zen/
```

Set the key before validation:

```bash
export OPENCODE_API_KEY="your-api-key"
```

PowerShell:

```powershell
$env:OPENCODE_API_KEY="your-api-key"
```

Generic hosted provider example:

```ini
[hosted-fast]
provider=openai-compatible
model=gpt-4.1-mini
base_url=https://api.openai.com/v1
api_key_env=OPENAI_API_KEY
reasoning=false
timeout_seconds=180
```

Local OpenAI-compatible providers are supported, but TermBullet does not
recommend a local model by default.

Local provider example:

```ini
[local-custom]
provider=openai-compatible
model=your-local-model
base_url=http://localhost:11434/v1
api_key=local
reasoning=false
test_max_tokens=64
chat_max_tokens=600
planning_max_tokens=1200
timeout_seconds=180
```

Required keys:

- `provider`
- `model`
- `base_url` for `openai-compatible`
- either `api_key` or `api_key_env`

Optional behavior keys:

- `reasoning`
- `test_max_tokens` (optional, for providers that require explicit limits)
- `chat_max_tokens` (optional, for providers that require explicit limits)
- `planning_max_tokens` (optional, for providers that require explicit limits)
- `timeout_seconds`

If a single profile exists, it is active automatically. If multiple profiles
exist, exactly one must have `default=true`.

### `test-ai`

```bash
termbullet test-ai
termbullet test-ai opencode-free
```

Validates `.aiconf`, resolves the active or requested profile, checks required
settings, and sends a short provider request. `test-ai` uses the profile's
`test_max_tokens`, which should be higher for reasoning models. A failed test
returns a clear actionable error.

### `set-ai`

```bash
termbullet set-ai opencode-free
```

Sets the default profile in `.aiconf`. It rewrites the file with the same profile
settings and updates `default=true`.

## V2 AI Planning Commands

### `ai plan`

```bash
termbullet ai plan new-project --prompt "Plan the billing module"
termbullet ai plan new-weekly --prompt "Organize my week"
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

Rules:

- The active AI profile is loaded from `<data_root>/.aiconf`.
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
model: deepseek-v4-flash-free
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
```

Allowed `--mode` values:

- `new-project`
- `new-weekly`

Interactive commands:

```text
/mode new-project
/mode new-weekly
/apply
/discard
/exit
```

Rules:

- `ai chat` uses the active profile from `<data_root>/.aiconf`.
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
