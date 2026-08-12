# Decisions - telegram-pm

Autonomous build decisions made while implementing this, per the operator's standing instruction to record assumptions here instead of asking. This supersedes the project's original Telegraf/Bot-API build (see git history) - the operator explicitly required MTProto via gramJS against a personal account instead, with a fuller 7-agent architecture, SQLite persistence, and a full command set. Rebuilt from scratch on that spec.

## Package choice: `telegram` (gramJS)

`npm install telegram` prints a deprecation notice - the package is archived upstream, with `teleproto` listed as the maintained fork. The operator explicitly named "gramJS (telegram npm package)" and explicitly forbade Telegraf/Bot API, so `telegram` was kept as specified rather than silently substituted. Flagging this so it's a visible tradeoff, not a silent one: if gramJS's archival becomes a practical problem later, `teleproto` is described as "largely compatible" and would be the natural migration target.

## SQLite via `node:sqlite`, not `better-sqlite3`

The spec said "Use SQLite initially." `better-sqlite3` (the conventional choice) requires native compilation via node-gyp, and this machine has no Visual Studio C++ build tools installed - `npm install` failed outright. Node 24's built-in `node:sqlite` (`DatabaseSync`) needed no native build, worked immediately, and has a very similar synchronous prepared-statement API. It's marked experimental by Node but is fully functional here. This is a pragmatic substitution to unblock the build, not a scope reduction - same on-disk SQLite file, same relational schema, same repository-pattern access.

## Architecture mapping

Telegram User Account → Telegram PM → Task Manager → Memory/State → Agent Orchestrator → [Planner, Backend, Frontend, QA, Research, Reviewer, DevOps] → Claude Agent SDK, per the requested diagram:

- **Telegram PM** (`src/telegram/`) - gramJS client, one-time interactive login script, an event listener scoped to `ALLOWED_CHAT_ID` (default: Saved Messages via `client.getMe()`), and a sender with Telegram's 4096-char message limit handled by chunking.
- **Task Manager** (`src/orchestrator/taskManager.ts`) - owns per-project run state (paused/cancelled/running), a global FIFO queue so distinct `/new`/`/retry` kickoffs don't race the same working directory, and `AbortController` plumbing so `/cancel` actually interrupts whatever agent call is in flight (not just "stops before the next subtask").
- **Memory/State** (`src/memory/memoryService.ts` + `agent_sessions` table) - two distinct things, both required by the spec: (1) durable cross-agent project memory (short key/value notes written after each successful subtask, prepended as context to later subtasks in the same project - this is `remember`/`recall`/`renderContext`), and (2) per-(project, agentType) Claude Agent SDK session IDs so each agent's own conversation history persists and resumes across runs within a project. These are deliberately separate: (1) is human-readable cross-agent knowledge, (2) is the SDK's own private transcript for one agent.
- **Agent Orchestrator** (`src/orchestrator/agentOrchestrator.ts`) - runs one task through its assigned agent, with automatic retry up to `MAX_ATTEMPTS_PER_TASK`, isolated-context bookkeeping (`agent_runs` rows), and cancellation awareness.
- **Planner/Backend/Frontend/QA/Research/Reviewer/DevOps** (`src/agents/`) - each is a distinct `AgentDefinition` (own system prompt, own restricted tool list) invoked as its **own top-level Claude Agent SDK `query()` call**, not via the SDK's internal `Task`-tool subagent delegation. This was a deliberate choice: the SDK's built-in subagents share one top-level session/transcript under the orchestrating call, which does not give "isolated context, independent execution, own Claude SDK session, own conversation history" per agent - the spec's explicit requirement. Running each agent as its own `query()` with its own resumed session ID (keyed by `(project, agentType)` in `agent_sessions`) does.
- **Claude Agent SDK** - `@anthropic-ai/claude-agent-sdk`, `query()` with `resume`, `permissionMode`, `canUseTool`, `outputFormat` (structured JSON for the Planner's subtask breakdown), and `abortController`.

Workflows (`src/workflows/`) compose these: `newProjectWorkflow.ts` is the full `/new` lifecycle (plan → create subtask rows → execute queue → final Reviewer pass → project status); `retryWorkflow.ts` is the `/retry` lifecycle (reset one task's attempt counter and session-continuity-preserving re-run).

## Destructive-action approval

Implemented via the SDK's `canUseTool` callback, not `permissionMode: bypassPermissions`. Every `Bash` tool call is checked against a curated regex list (`rm -rf`, `git push --force`, `git reset --hard`, `DROP TABLE`, `docker volume rm`, `kubectl delete`, etc. - see `src/utils/destructiveCommand.ts`). A match creates a persisted `Approval` row, notifies the operator in Telegram with the exact command and the approval's id, and **awaits** an in-memory `Promise` that only resolves via `/approve <id>` or `/reject <id>`. This literally pauses that agent call mid-execution until the operator responds - not a polling loop. Non-destructive tool calls (including all Read/Write/Edit for non-Bash tools) are auto-allowed, matching "ask for approval only before destructive actions" precisely rather than gating everything or nothing.

The wait is also raced against the SDK's `AbortSignal` (see Cancellation below) so a stuck approval doesn't survive a `/cancel`.

## Cancellation is a real interrupt, not just "stop queueing"

Initially `/cancel` only prevented the *next* subtask from starting - a subtask already mid-execution (which can run for minutes, or be stuck on an approval wait) would run to completion regardless. Fixed by threading a per-project `AbortController` through `agentOrchestrator.executeTask` → `runAgent` → the SDK's `options.abortController`, created fresh before every agent call (planning, each subtask, the final review) and aborted by `cancelProject()`. The `canUseTool` approval wait is separately raced against the same abort signal so a cancel unblocks it immediately rather than leaving the run hung. A cancelled task is recorded with status `cancelled` (distinct from `failed`, so it doesn't count against the operator's mental model of "things that broke").

## Security posture

- **`ALLOWED_CHAT_ID` (default: your own Saved Messages, resolved via `client.getMe()`) is the entire access boundary.** Every incoming Telegram update is filtered against it before anything reaches a command handler. Whoever can write to that chat can run arbitrary shell commands and edit arbitrary files in the working directory - this is a single-operator tool, not a multi-user bot.
- **`permissionMode: 'default'` is intentional, not a bug** - all real tool-permission decisions are made by the `canUseTool` callback described above, which is always provided. `bypassPermissions` would skip that callback's role entirely and remove the approval gate.
- **`MAX_TURNS_PER_AGENT_RUN` and `MAX_BUDGET_USD_PER_AGENT_RUN`** (default 100 turns / $3, per single agent call) bound runaway cost/loops, since nothing else limits an autonomous agent working unattended.
- The MTProto session string (`data/telegram.session`) is your live login for your personal Telegram account, equivalent in sensitivity to a password - gitignored, written with `0o600` permissions, never logged.

## Model and SDK

- Model defaults to `claude-opus-4-8` (current guidance: default to the latest Opus unless told otherwise). Configurable via `CLAUDE_MODEL`.
- Backend and Frontend agents default their `cwd` to `backend/`/`frontend/` when those directories exist in the working directory (matching this repo's layout), falling back to the repo root otherwise - see `src/utils/paths.ts`.
- `settingSources: ['project']` so agents load this repo's own `CLAUDE.md` if present, rather than running in full SDK isolation.
- The Planner's subtask breakdown uses the SDK's `outputFormat: {type: 'json_schema', ...}` rather than asking it to format plain text and regex-parsing the response - more robust, and the result lands typed in `SDKResultMessage.structured_output`.

## Command → implicit /new

A message with no leading `/` is treated as shorthand for `/new <message>`. The spec lists `/new` as a formal command but the natural UX for "just tell your PM what you want" is to not require typing `/new` every time; both are supported.

## Deferred / not built

- No inline Telegram buttons for `/approve`/`/reject` - text commands with the approval ID are sufficient and simpler; buttons would be the natural next step if the operator wants one-tap approval.
- Approval promises are in-memory only; a restart while one is pending orphans it (documented in README limitations) - persisting a resumable wait would need a fundamentally different mechanism (e.g. polling the DB instead of an awaited Promise) and wasn't justified for a single-operator tool where restarts during an active destructive-approval wait should be rare.
