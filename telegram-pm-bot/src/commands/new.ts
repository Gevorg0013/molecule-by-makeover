import type { CommandHandler } from './types.js'
import * as taskManager from '../orchestrator/taskManager.js'
import { startNewProject } from '../workflows/newProjectWorkflow.js'
import { formatToolUse } from '../utils/telegramFormat.js'
import type { Approval } from '../database/types.js'

function deriveName(description: string): string {
  const firstLine = description.split('\n')[0]?.trim() ?? description.trim()
  return firstLine.length > 60 ? `${firstLine.slice(0, 57)}...` : firstLine
}

export const newCommand: CommandHandler = async (ctx) => {
  const description = ctx.args.trim()
  if (!description) {
    await ctx.reply('Usage: /new <what you want built>')
    return
  }

  const queueLength = taskManager.queuedRunCount()
  if (queueLength > 0) {
    await ctx.reply(`Queued behind ${queueLength} other project(s) already waiting.`)
  }

  await taskManager.enqueueGlobalRun('new-project', async () => {
    await ctx.reply(`New project: "${deriveName(description)}"\n\n📝 Planning...`)

    const result = await startNewProject(deriveName(description), description, {
      onPhase: async (phase) => {
        const label = phase === 'planning' ? '📝 Planning' : phase === 'executing' ? '⚙️ Executing subtasks' : '🔍 Final review'
        await ctx.reply(label)
      },
      onPlanReady: async (subtasks) => {
        const lines = subtasks.map((s, i) => `${i + 1}. [${s.agent}] ${s.title}`)
        await ctx.reply(`Plan (${subtasks.length} subtasks):\n${lines.join('\n')}`)
      },
      onSubtaskStarted: async (title, agentType) => {
        await ctx.reply(`▶️ [${agentType}] ${title}`)
      },
      onSubtaskFinished: async (title, agentType, success) => {
        await ctx.reply(`${success ? '✅' : '❌'} [${agentType}] ${title}`)
      },
      onAttemptFailed: async (attempt, maxAttempts, error) => {
        await ctx.reply(`⚠️ Attempt ${attempt}/${maxAttempts} failed: ${error.slice(0, 500)}`)
      },
      onToolUse: async (toolName, input) => {
        await ctx.reply(formatToolUse(toolName, input))
      },
      onApprovalNeeded: async (approval: Approval) => {
        await ctx.reply(
          `🛑 Approval needed (id: ${approval.id})\n${approval.description}\n\nCommand: ${approval.command ?? '(n/a)'}\n\nReply /approve ${approval.id} or /reject ${approval.id}`,
        )
      },
    })

    if (!result.ok && result.reason && result.completed === undefined) {
      await ctx.reply(`❌ Project failed to start: ${result.reason}`)
      return
    }

    const summaryLine = `Project ${result.ok ? 'completed' : 'needs review'}. ${result.completed ?? 0} subtask(s) succeeded, ${result.failed ?? 0} failed.`
    await ctx.reply(`${result.ok ? '✅' : '⚠️'} ${summaryLine}${result.reviewSummary ? `\n\nReview: ${result.reviewSummary}` : ''}`)
  })
}
