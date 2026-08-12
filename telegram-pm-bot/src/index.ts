import './database/db.js'
import { config } from './config/env.js'
import { createTelegramClient } from './telegram/client.js'
import { bindClient, send } from './telegram/sender.js'
import { startListening } from './telegram/listener.js'
import * as taskService from './services/taskService.js'
import * as agentRunService from './services/agentRunService.js'
import { logger } from './utils/logger.js'

async function main(): Promise<void> {
  const orphanedTasks = taskService.resetOrphanedRunning()
  const orphanedRuns = agentRunService.closeOrphanedRuns()
  if (orphanedTasks > 0) {
    logger.warn(`Marked ${orphanedTasks} task(s) failed after an unclean restart - use /retry to resume them.`)
  }
  if (orphanedRuns > 0) {
    logger.warn(`Closed ${orphanedRuns} orphaned agent run(s) from before restart.`)
  }

  const client = await createTelegramClient()
  bindClient(client)
  await startListening(client)

  logger.info(`PM online. Working directory: ${config.workingDir}`)
  await send(
    `PM online. Working directory: ${config.workingDir}${orphanedTasks > 0 ? `\n\n⚠️ ${orphanedTasks} task(s) were interrupted by a restart and marked failed - /tasks to see them, /retry to resume.` : ''}`,
  )
}

main().catch((err) => {
  logger.error('Failed to start PM', err)
  process.exit(1)
})

process.once('SIGINT', () => process.exit(0))
process.once('SIGTERM', () => process.exit(0))
