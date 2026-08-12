import type { CommandHandler } from './types.js'
import { newCommand } from './new.js'
import { statusCommand } from './status.js'
import { tasksCommand } from './tasks.js'
import { projectsCommand } from './projects.js'
import { agentsCommand } from './agents.js'
import { logsCommand } from './logs.js'
import { retryCommand } from './retry.js'
import { cancelCommand } from './cancel.js'
import { pauseCommand } from './pause.js'
import { resumeCommand } from './resume.js'
import { approveCommand } from './approve.js'
import { rejectCommand } from './reject.js'
import { helpCommand } from './help.js'
import { send } from '../telegram/sender.js'
import { logger } from '../utils/logger.js'

const handlers: Record<string, CommandHandler> = {
  new: newCommand,
  status: statusCommand,
  tasks: tasksCommand,
  projects: projectsCommand,
  agents: agentsCommand,
  logs: logsCommand,
  retry: retryCommand,
  cancel: cancelCommand,
  pause: pauseCommand,
  resume: resumeCommand,
  approve: approveCommand,
  reject: rejectCommand,
  help: helpCommand,
  start: helpCommand,
}

const COMMAND_PATTERN = /^\/(\w+)\s*([\s\S]*)$/

export async function dispatch(rawText: string): Promise<void> {
  const text = rawText.trim()
  if (!text) return

  const match = text.match(COMMAND_PATTERN)
  if (match) {
    const [, name, rest] = match
    const handler = handlers[name.toLowerCase()]
    if (!handler) {
      await send(`Unknown command /${name}. Send /help for the list of commands.`)
      return
    }
    try {
      await handler({ args: rest.trim(), reply: send })
    } catch (err) {
      logger.error(`Command /${name} failed`, err)
      await send(`❌ /${name} failed: ${err instanceof Error ? err.message : String(err)}`)
    }
    return
  }

  // Plain text with no leading command is treated as shorthand for /new.
  try {
    await newCommand({ args: text, reply: send })
  } catch (err) {
    logger.error('Implicit /new failed', err)
    await send(`❌ Failed: ${err instanceof Error ? err.message : String(err)}`)
  }
}
