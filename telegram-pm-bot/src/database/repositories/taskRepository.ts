import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { AgentType, Task, TaskStatus } from '../types.js'

function mapRow(row: Record<string, unknown>): Task {
  return {
    id: row.id as string,
    projectId: row.project_id as string,
    parentTaskId: (row.parent_task_id as string | null) ?? null,
    title: row.title as string,
    description: row.description as string,
    agentType: row.agent_type as AgentType,
    status: row.status as TaskStatus,
    orderIndex: Number(row.order_index),
    attemptCount: Number(row.attempt_count),
    maxAttempts: Number(row.max_attempts),
    resultSummary: (row.result_summary as string | null) ?? null,
    error: (row.error as string | null) ?? null,
    createdAt: row.created_at as string,
    updatedAt: row.updated_at as string,
  }
}

export interface CreateTaskInput {
  projectId: string
  parentTaskId: string | null
  title: string
  description: string
  agentType: AgentType
  orderIndex: number
  maxAttempts: number
}

export function createTask(input: CreateTaskInput): Task {
  const id = newId()
  const now = nowIso()
  db.prepare(
    `INSERT INTO tasks (id, project_id, parent_task_id, title, description, agent_type, status, order_index, attempt_count, max_attempts, created_at, updated_at)
     VALUES (?, ?, ?, ?, ?, ?, 'pending', ?, 0, ?, ?, ?)`,
  ).run(
    id,
    input.projectId,
    input.parentTaskId,
    input.title,
    input.description,
    input.agentType,
    input.orderIndex,
    input.maxAttempts,
    now,
    now,
  )
  return {
    id,
    projectId: input.projectId,
    parentTaskId: input.parentTaskId,
    title: input.title,
    description: input.description,
    agentType: input.agentType,
    status: 'pending',
    orderIndex: input.orderIndex,
    attemptCount: 0,
    maxAttempts: input.maxAttempts,
    resultSummary: null,
    error: null,
    createdAt: now,
    updatedAt: now,
  }
}

export function getTask(id: string): Task | undefined {
  const row = db.prepare(`SELECT * FROM tasks WHERE id = ?`).get(id)
  return row ? mapRow(row) : undefined
}

export function listTasksForProject(projectId: string): Task[] {
  const rows = db.prepare(`SELECT * FROM tasks WHERE project_id = ? ORDER BY order_index ASC`).all(projectId)
  return rows.map(mapRow)
}

export function listSubtasks(parentTaskId: string): Task[] {
  const rows = db.prepare(`SELECT * FROM tasks WHERE parent_task_id = ? ORDER BY order_index ASC`).all(parentTaskId)
  return rows.map(mapRow)
}

export function listRecentTasks(limit = 10): Task[] {
  const rows = db.prepare(`SELECT * FROM tasks ORDER BY created_at DESC LIMIT ?`).all(limit)
  return rows.map(mapRow)
}

export function updateTaskStatus(id: string, status: TaskStatus, extra?: { resultSummary?: string; error?: string }): void {
  db.prepare(
    `UPDATE tasks SET status = ?, result_summary = COALESCE(?, result_summary), error = COALESCE(?, error), updated_at = ? WHERE id = ?`,
  ).run(status, extra?.resultSummary ?? null, extra?.error ?? null, nowIso(), id)
}

export function incrementAttempt(id: string): void {
  db.prepare(`UPDATE tasks SET attempt_count = attempt_count + 1, updated_at = ? WHERE id = ?`).run(nowIso(), id)
}

export function resetAttempts(id: string): void {
  db.prepare(`UPDATE tasks SET attempt_count = 0, error = NULL, updated_at = ? WHERE id = ?`).run(nowIso(), id)
}

/** On startup: a task left 'running' means the process crashed/restarted mid-execution. Mark it failed so it surfaces for an explicit /retry rather than being silently re-run. */
export function resetOrphanedRunning(): number {
  const result = db
    .prepare(`UPDATE tasks SET status = 'failed', error = 'Interrupted by restart', updated_at = ? WHERE status = 'running'`)
    .run(nowIso())
  return Number(result.changes)
}
