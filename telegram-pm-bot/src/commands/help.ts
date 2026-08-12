import type { CommandHandler } from './types.js'

const HELP_TEXT = `I'm your autonomous PM. Send a plain message or /new <description> to start a project - I'll plan it, assign subtasks to the right agent, execute, and report back.

/new <description> - start a new project
/status - current project status and pending approvals
/tasks - subtasks for the current project
/projects - recent projects
/agents - the agent roster
/logs - recent activity log
/retry [taskId] - retry a failed task (defaults to the most recent failure)
/cancel [projectId] - stop after the current subtask
/pause [projectId] - pause after the current subtask
/resume [projectId] - resume a paused project
/approve <approvalId> - approve a pending destructive action
/reject <approvalId> - reject a pending destructive action
/help - this message`

export const helpCommand: CommandHandler = async (ctx) => {
  await ctx.reply(HELP_TEXT)
}
