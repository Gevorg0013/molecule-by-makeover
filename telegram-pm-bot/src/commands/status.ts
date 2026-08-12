import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskService from '../services/taskService.js'
import * as taskManager from '../orchestrator/taskManager.js'
import * as approvalService from '../services/approvalService.js'

export const statusCommand: CommandHandler = async (ctx) => {
  const project = projectService.getLatestProject()
  if (!project) {
    await ctx.reply('No projects yet. Send /new <description> to start one.')
    return
  }

  const tasks = taskService.listTasksForProject(project.id)
  const subtasks = tasks.filter((t) => t.parentTaskId !== null)
  const done = subtasks.filter((t) => t.status === 'success').length
  const failed = subtasks.filter((t) => t.status === 'failed').length
  const running = taskManager.isRunning(project.id)
  const paused = taskManager.isPaused(project.id)
  const pendingApprovals = approvalService.listPending()
  const queueLength = taskManager.queuedRunCount()

  const lines = [
    `Project: ${project.name}`,
    `Status: ${project.status}${paused ? ' (paused)' : ''}`,
    `Running: ${running ? 'yes' : 'no'}`,
    `Subtasks: ${done}/${subtasks.length} done, ${failed} failed`,
    `Queued new-project runs: ${queueLength}`,
  ]
  if (pendingApprovals.length > 0) {
    lines.push('', `Pending approvals: ${pendingApprovals.length}`, ...pendingApprovals.map((a) => `  ${a.id}: ${a.description}`))
  }

  await ctx.reply(lines.join('\n'))
}
