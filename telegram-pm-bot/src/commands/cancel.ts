import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskManager from '../orchestrator/taskManager.js'

export const cancelCommand: CommandHandler = async (ctx) => {
  const projectId = ctx.args.trim() || projectService.getLatestProject()?.id
  if (!projectId) {
    await ctx.reply('No project to cancel.')
    return
  }
  taskManager.cancelProject(projectId)
  await ctx.reply('🚫 Cancelling after the current subtask finishes.')
}
