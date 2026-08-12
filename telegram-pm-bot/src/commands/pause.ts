import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskManager from '../orchestrator/taskManager.js'

export const pauseCommand: CommandHandler = async (ctx) => {
  const projectId = ctx.args.trim() || projectService.getLatestProject()?.id
  if (!projectId) {
    await ctx.reply('No project to pause.')
    return
  }
  taskManager.pauseProject(projectId)
  projectService.setStatus(projectId, 'paused')
  await ctx.reply('⏸️ Pausing after the current subtask finishes.')
}
