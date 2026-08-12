import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { LogEntry, LogLevel } from '../types.js'

function mapRow(row: Record<string, unknown>): LogEntry {
  return {
    id: row.id as string,
    projectId: (row.project_id as string | null) ?? null,
    taskId: (row.task_id as string | null) ?? null,
    agentRunId: (row.agent_run_id as string | null) ?? null,
    level: row.level as LogLevel,
    message: row.message as string,
    createdAt: row.created_at as string,
  }
}

export interface AddLogInput {
  projectId?: string | null
  taskId?: string | null
  agentRunId?: string | null
  level: LogLevel
  message: string
}

export function addLog(input: AddLogInput): void {
  db.prepare(
    `INSERT INTO logs (id, project_id, task_id, agent_run_id, level, message, created_at) VALUES (?, ?, ?, ?, ?, ?, ?)`,
  ).run(newId(), input.projectId ?? null, input.taskId ?? null, input.agentRunId ?? null, input.level, input.message, nowIso())
}

export function listRecentLogs(limit = 30, projectId?: string): LogEntry[] {
  const rows = projectId
    ? db.prepare(`SELECT * FROM logs WHERE project_id = ? ORDER BY created_at DESC LIMIT ?`).all(projectId, limit)
    : db.prepare(`SELECT * FROM logs ORDER BY created_at DESC LIMIT ?`).all(limit)
  return rows.map(mapRow)
}
