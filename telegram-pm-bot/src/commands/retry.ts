import type { CommandHandler } from './types.js'
import * as projectService from '../services/projectService.js'
import * as taskService from '../services/taskService.js'
import * as taskManager from '../orchestrator/taskManager.js'
import { retryTask, findMostRecentFailedTask } from '../workflows/retryWorkflow.js'
import { formatToolUse } from '../utils/telegramFormat.js'

export const retryCommand: CommandHandler = async (ctx) => {
  const explicitId = ctx.args.trim()
  const task = explicitId ? taskService.getTask(explicitId) : findMostRecentFailedTask(projectService.getLatestProject()?.id)

  if (!task) {
    await ctx.reply(explicitId ? `No task found with id ${explicitId}.` : 'No failed tasks to retry.')
    return
  }

  await taskManager.enqueueGlobalRun('retry', async () => {
    await ctx.reply(`🔁 Retrying [${task.agentType}] ${task.title}`)
    const result = await retryTask(task.id, {
      onText: async (text) => ctx.reply(text),
      onToolUse: async (toolName, input) => ctx.reply(formatToolUse(toolName, input)),
      onApprovalNeeded: async (approval) =>
        ctx.reply(`🛑 Approval needed (id: ${approval.id})\n${approval.description}\n\nReply /approve ${approval.id} or /reject ${approval.id}`),
      onAttemptFailed: async (attempt, maxAttempts, error) => ctx.reply(`⚠️ Attempt ${attempt}/${maxAttempts} failed: ${error.slice(0, 500)}`),
    })
    await ctx.reply(result.success ? `✅ Retry succeeded: ${task.title}` : `❌ Retry failed: ${result.error ?? 'unknown error'}`)
  })
}
