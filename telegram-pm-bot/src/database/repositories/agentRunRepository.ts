import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { AgentRun, AgentRunStatus, AgentType } from '../types.js'

function mapRow(row: Record<string, unknown>): AgentRun {
  return {
    id: row.id as string,
    taskId: row.task_id as string,
    agentType: row.agent_type as AgentType,
    claudeSessionId: (row.claude_session_id as string | null) ?? null,
    status: row.status as AgentRunStatus,
    startedAt: row.started_at as string,
    finishedAt: (row.finished_at as string | null) ?? null,
    costUsd: row.cost_usd === null ? null : Number(row.cost_usd),
    numTurns: row.num_turns === null ? null : Number(row.num_turns),
    error: (row.error as string | null) ?? null,
    summary: (row.summary as string | null) ?? null,
  }
}

export function startAgentRun(taskId: string, agentType: AgentType): AgentRun {
  const id = newId()
  const now = nowIso()
  db.prepare(
    `INSERT INTO agent_runs (id, task_id, agent_type, status, started_at) VALUES (?, ?, ?, 'running', ?)`,
  ).run(id, taskId, agentType, now)
  return {
    id,
    taskId,
    agentType,
    claudeSessionId: null,
    status: 'running',
    startedAt: now,
    finishedAt: null,
    costUsd: null,
    numTurns: null,
    error: null,
    summary: null,
  }
}

export function finishAgentRun(
  id: string,
  result: {
    status: AgentRunStatus
    claudeSessionId?: string
    costUsd?: number
    numTurns?: number
    error?: string
    summary?: string
  },
): void {
  db.prepare(
    `UPDATE agent_runs SET status = ?, claude_session_id = COALESCE(?, claude_session_id), cost_usd = ?, num_turns = ?, error = ?, summary = ?, finished_at = ? WHERE id = ?`,
  ).run(
    result.status,
    result.claudeSessionId ?? null,
    result.costUsd ?? null,
    result.numTurns ?? null,
    result.error ?? null,
    result.summary ?? null,
    nowIso(),
    id,
  )
}

export function listRecentRuns(limit = 10): AgentRun[] {
  const rows = db.prepare(`SELECT * FROM agent_runs ORDER BY started_at DESC LIMIT ?`).all(limit)
  return rows.map(mapRow)
}

/** Companion to taskRepository.resetOrphanedRunning() - closes out the matching agent_runs rows. */
export function closeOrphanedRuns(): number {
  const result = db
    .prepare(`UPDATE agent_runs SET status = 'error', error = 'Interrupted by restart', finished_at = ? WHERE status = 'running'`)
    .run(nowIso())
  return Number(result.changes)
}
