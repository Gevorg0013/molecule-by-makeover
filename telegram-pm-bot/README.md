# Telegram PM

An autonomous AI project manager that lives in your own Telegram account's **Saved Messages** (or a chat you configure). It plans, delegates to a roster of specialist agents (Planner, Backend, Frontend, QA, Research, Reviewer, DevOps), executes, retries failures automatically, and reports back - all driven by the Claude Agent SDK.

Uses **MTProto via gramJS** (the `telegram` package) against your personal account - no Bot API, no BotFather, no bot token. See `DECISIONS.md` for the full architecture and every autonomous decision made while building this.

> ⚠️ This runs real shell commands and edits real files in this repo based on messages in your Telegram account. The security boundary is `ALLOWED_CHAT_ID` (defaults to your own Saved Messages) plus the MTProto session file - treat both, and your `.env`, as sensitive.

## 1. Get your API credentials (one-time, ~2 minutes)

1. Go to https://my.telegram.org/apps and log in with your phone number.
2. Create an app (any name/platform is fine). You'll get an **App api_id** and **App api_hash**.
3. Put them in `.env` as `TELEGRAM_API_ID` and `TELEGRAM_API_HASH`.

## 2. Configure

```bash
cd telegram-pm-bot
cp .env.example .env
# fill in TELEGRAM_API_ID, TELEGRAM_API_HASH, ANTHROPIC_API_KEY
npm install
```

## 3. Log in (one-time)

```bash
npm run login
```

This prompts for your phone number, the login code Telegram sends to your existing devices, and your 2FA password if you have one set. On success it writes a `StringSession` to `data/telegram.session` (gitignored) - every future `npm run dev` / `npm start` reuses it silently, no login prompt again unless that file is deleted or Telegram invalidates the session.

## 4. Run

```bash
npm run dev      # ts-node style dev run
# or
npm run build && npm start
```

On startup it logs in with the saved session, listens on your Saved Messages chat (or `ALLOWED_CHAT_ID` if you set one), and posts a "PM online" message there.

## 5. Use it

Open **Saved Messages** in Telegram and send a plain description of what you want built, or use `/new <description>` explicitly. The PM will:

1. Plan the work (Planner agent) into an ordered list of subtasks, each assigned to Backend / Frontend / QA / Research / Reviewer / DevOps.
2. Execute them one at a time, retrying each up to `MAX_ATTEMPTS_PER_TASK` on failure.
3. Pause and ask for `/approve` / `/reject` before any subtask runs a command that looks destructive (`rm -rf`, `git push --force`, `DROP TABLE`, etc.).
4. Run a final Reviewer pass over the full diff.
5. Report progress live and a final summary, all in the same chat.

### Commands

```
/new <description>   start a new project
/status               current project status, running state, pending approvals
/tasks                subtasks for the current project
/projects              recent projects
/agents                 the agent roster
/logs                  recent activity log + recent agent runs
/retry [taskId]        retry a failed task (defaults to the most recent failure)
/cancel [projectId]    stop after the current subtask AND interrupt whatever's running right now
/pause [projectId]     pause after the current subtask finishes
/resume [projectId]    resume a paused project
/approve <approvalId>  approve a pending destructive action
/reject <approvalId>   reject a pending destructive action
/help                  this list
```

Plain text with no leading `/` is treated as shorthand for `/new`.

## Data

Everything (projects, tasks, agent runs, memory, logs, approvals, per-agent Claude session pointers) lives in a local SQLite database at `data/pm.sqlite` (Node's built-in `node:sqlite`, no native build step). The process survives restarts: any task still marked `running` when the process exits uncleanly is marked `failed` on the next startup so you can `/retry` it explicitly rather than it silently re-running.

## Limitations

- Destructive-command detection is a curated regex list on `Bash` calls (see `src/utils/destructiveCommand.ts`) - it is not exhaustive, and it doesn't gate `Write`/`Edit` calls that overwrite or delete files outright.
- A pending `/approve`/`/reject` is held in memory; if the process restarts while one is outstanding, resolve it by `/retry`-ing the affected task instead (the approval row is still visible in `/status`, but nothing is listening for its resolution anymore).
- One operator, one active project kickoff at a time by design (`/new`/`/retry` calls queue globally) - this is intentional given the single-Saved-Messages-chat model, not a scaling limitation to fix.
