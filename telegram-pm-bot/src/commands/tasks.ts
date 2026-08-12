import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskService from '../services/taskService.js'

const STATUS_ICON: Record<string, string> = {
  pending: '⏳',
  running: '▶️',
  blocked_approval: '🛑',
  paused: '⏸️',
  success: '✅',
  failed: '❌',
  cancelled: '🚫',
}

export const tasksCommand: CommandHandler = async (ctx) => {
  const project = projectService.getLatestProject()
  if (!project) {
    await ctx.reply('No projects yet. Send /new <description> to start one.')
    return
  }

  const tasks = taskService.listTasksForProject(project.id).filter((t) => t.parentTaskId !== null)
  if (tasks.length === 0) {
    await ctx.reply(`Project "${project.name}" has no subtasks yet.`)
    return
  }

  const lines = tasks.map((t) => `${STATUS_ICON[t.status] ?? '•'} [${t.agentType}] ${t.title} (${t.attemptCount}/${t.maxAttempts} attempts)`)
  await ctx.reply(`Tasks for "${project.name}":\n${lines.join('\n')}`)
}
