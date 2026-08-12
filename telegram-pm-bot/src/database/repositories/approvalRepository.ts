import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { Approval, ApprovalStatus } from '../types.js'

function mapRow(row: Record<string, unknown>): Approval {
  return {
    id: row.id as string,
    projectId: (row.project_id as string | null) ?? null,
    taskId: (row.task_id as string | null) ?? null,
    description: row.description as string,
    command: (row.command as string | null) ?? null,
    status: row.status as ApprovalStatus,
    requestedAt: row.requested_at as string,
    resolvedAt: (row.resolved_at as string | null) ?? null,
    resolvedBy: (row.resolved_by as string | null) ?? null,
  }
}

export function createApproval(input: {
  projectId?: string | null
  taskId?: string | null
  description: string
  command?: string | null
}): Approval {
  const id = newId()
  const now = nowIso()
  db.prepare(
    `INSERT INTO approvals (id, project_id, task_id, description, command, status, requested_at) VALUES (?, ?, ?, ?, ?, 'pending', ?)`,
  ).run(id, input.projectId ?? null, input.taskId ?? null, input.description, input.command ?? null, now)
  return {
    id,
    projectId: input.projectId ?? null,
    taskId: input.taskId ?? null,
    description: input.description,
    command: input.command ?? null,
    status: 'pending',
    requestedAt: now,
    resolvedAt: null,
    resolvedBy: null,
  }
}

export function getApproval(id: string): Approval | undefined {
  const row = db.prepare(`SELECT * FROM approvals WHERE id = ?`).get(id)
  return row ? mapRow(row) : undefined
}

export function resolveApproval(id: string, status: 'approved' | 'rejected', resolvedBy: string): void {
  db.prepare(`UPDATE approvals SET status = ?, resolved_at = ?, resolved_by = ? WHERE id = ?`).run(
    status,
    nowIso(),
    resolvedBy,
    id,
  )
}

export function listPendingApprovals(): Approval[] {
  const rows = db.prepare(`SELECT * FROM approvals WHERE status = 'pending' ORDER BY requested_at ASC`).all()
  return rows.map(mapRow)
}
