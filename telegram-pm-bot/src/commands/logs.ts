import type { CommandHandler } from './types.js'
import * as logService from '../services/logService.js'
import * as projectService from '../services/projectService.js'
import * as agentRunService from '../services/agentRunService.js'

export const logsCommand: CommandHandler = async (ctx) => {
  const project = projectService.getLatestProject()
  const logs = logService.recentLogs(20, project?.id)
  const runs = agentRunService.listRecentRuns(10)

  const parts: string[] = []

  if (logs.length > 0) {
    const lines = logs
      .slice()
      .reverse()
      .map((l) => `[${l.level}] ${l.message}`)
    parts.push(lines.join('\n'))
  }

  if (runs.length > 0) {
    const runLines = runs.map((r) => {
      const cost = r.costUsd !== null ? `$${r.costUsd.toFixed(3)}` : '-'
      const duration = r.finishedAt ? `${((Date.parse(r.finishedAt) - Date.parse(r.startedAt)) / 1000).toFixed(0)}s` : 'running'
      return `[${r.agentType}] ${r.status} - ${cost}, ${duration}`
    })
    parts.push(`Recent agent runs:\n${runLines.join('\n')}`)
  }

  await ctx.reply(parts.length > 0 ? parts.join('\n\n') : 'No logs yet.')
}
