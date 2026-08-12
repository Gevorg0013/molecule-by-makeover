import path from 'node:path'
import { fileURLToPath } from 'node:url'
import dotenv from 'dotenv'

dotenv.config()

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const projectRoot = path.join(__dirname, '..', '..')

function required(name: string): string {
  const value = process.env[name]
  if (!value) throw new Error(`Missing required env var: ${name}`)
  return value
}

export const config = {
  telegramApiId: Number(required('TELEGRAM_API_ID')),
  telegramApiHash: required('TELEGRAM_API_HASH'),
  /** "me" = Saved Messages (default). Set to a numeric Telegram user/chat ID to listen elsewhere instead. */
  allowedChatId: process.env.ALLOWED_CHAT_ID?.trim() || 'me',
  anthropicApiKey: required('ANTHROPIC_API_KEY'),
  sessionFile: process.env.SESSION_FILE ?? path.join(projectRoot, 'data', 'telegram.session'),
  dbFile: process.env.DB_FILE ?? path.join(projectRoot, 'data', 'pm.sqlite'),
  workingDir: path.resolve(process.env.WORKING_DIR ?? path.join(projectRoot, '..')),
  model: process.env.CLAUDE_MODEL ?? 'claude-opus-4-8',
  /**
   * Tool permission decisions are actually made by the canUseTool callback in
   * agents/agentExecutor.ts (allow everything except destructive commands,
   * which pause for a Telegram /approve|/reject). This mode is the SDK's
   * fallback for anything canUseTool doesn't cover; 'default' is correct
   * because canUseTool is always provided. Override only if you understand
   * the interaction with canUseTool.
   */
  permissionMode: (process.env.PERMISSION_MODE ?? 'default') as 'default' | 'acceptEdits' | 'bypassPermissions' | 'plan',
  maxTurnsPerAgentRun: Number(process.env.MAX_TURNS_PER_AGENT_RUN ?? 100),
  maxBudgetUsdPerAgentRun: process.env.MAX_BUDGET_USD_PER_AGENT_RUN
    ? Number(process.env.MAX_BUDGET_USD_PER_AGENT_RUN)
    : 3,
  maxAttemptsPerTask: Number(process.env.MAX_ATTEMPTS_PER_TASK ?? 3),
  projectRoot,
}
