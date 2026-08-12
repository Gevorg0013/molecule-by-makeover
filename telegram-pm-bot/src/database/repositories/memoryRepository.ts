import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { MemoryEntry } from '../types.js'

function mapRow(row: Record<string, unknown>): MemoryEntry {
  return {
    id: row.id as string,
    projectId: (row.project_id as string | null) ?? null,
    taskId: (row.task_id as string | null) ?? null,
    key: row.key as string,
    value: row.value as string,
    createdAt: row.created_at as string,
    updatedAt: row.updated_at as string,
  }
}

export function upsertMemory(projectId: string, key: string, value: string, taskId: string | null = null): void {
  const existing = db
    .prepare(`SELECT id FROM memory WHERE project_id = ? AND key = ?`)
    .get(projectId, key) as { id: string } | undefined
  const now = nowIso()
  if (existing) {
    db.prepare(`UPDATE memory SET value = ?, task_id = ?, updated_at = ? WHERE id = ?`).run(value, taskId, now, existing.id)
  } else {
    db.prepare(
      `INSERT INTO memory (id, project_id, task_id, key, value, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?, ?)`,
    ).run(newId(), projectId, taskId, key, value, now, now)
  }
}

export function listMemoryForProject(projectId: string): MemoryEntry[] {
  const rows = db.prepare(`SELECT * FROM memory WHERE project_id = ? ORDER BY updated_at DESC`).all(projectId)
  return rows.map(mapRow)
}
