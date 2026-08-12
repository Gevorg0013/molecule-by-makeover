import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskManager from '../orchestrator/taskManager.js'

export const resumeCommand: CommandHandler = async (ctx) => {
  const projectId = ctx.args.trim() || projectService.getLatestProject()?.id
  if (!projectId) {
    await ctx.reply('No project to resume.')
    return
  }
  taskManager.resumeProject(projectId)
  projectService.setStatus(projectId, 'running')
  await ctx.reply('▶️ Resumed.')
}
