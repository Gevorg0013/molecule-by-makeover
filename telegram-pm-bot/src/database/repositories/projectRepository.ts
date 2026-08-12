import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { Project, ProjectStatus } from '../types.js'

function mapRow(row: Record<string, unknown>): Project {
  return {
    id: row.id as string,
    name: row.name as string,
    description: row.description as string,
    status: row.status as ProjectStatus,
    createdAt: row.created_at as string,
    updatedAt: row.updated_at as string,
  }
}

export function createProject(name: string, description: string): Project {
  const id = newId()
  const now = nowIso()
  db.prepare(
    `INSERT INTO projects (id, name, description, status, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?)`,
  ).run(id, name, description, 'planning', now, now)
  return { id, name, description, status: 'planning', createdAt: now, updatedAt: now }
}

export function getProject(id: string): Project | undefined {
  const row = db.prepare(`SELECT * FROM projects WHERE id = ?`).get(id)
  return row ? mapRow(row) : undefined
}

export function listProjects(limit = 20): Project[] {
  const rows = db.prepare(`SELECT * FROM projects ORDER BY created_at DESC LIMIT ?`).all(limit)
  return rows.map(mapRow)
}

export function getLatestProject(): Project | undefined {
  const row = db.prepare(`SELECT * FROM projects ORDER BY created_at DESC LIMIT 1`).get()
  return row ? mapRow(row) : undefined
}

export function updateProjectStatus(id: string, status: ProjectStatus): void {
  db.prepare(`UPDATE projects SET status = ?, updated_at = ? WHERE id = ?`).run(status, nowIso(), id)
}
