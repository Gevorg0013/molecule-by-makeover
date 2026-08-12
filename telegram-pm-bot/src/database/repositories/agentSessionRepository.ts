import { db } from '../db.js'
import { newId, nowIso } from '../../utils/id.js'
import type { AgentType } from '../types.js'

export function getAgentSessionId(projectId: string, agentType: AgentType): string | undefined {
  const row = db
    .prepare(`SELECT claude_session_id FROM agent_sessions WHERE project_id = ? AND agent_type = ?`)
    .get(projectId, agentType) as { claude_session_id: string } | undefined
  return row?.claude_session_id
}

export function setAgentSessionId(projectId: string, agentType: AgentType, claudeSessionId: string): void {
  const now = nowIso()
  const existing = db
    .prepare(`SELECT id FROM agent_sessions WHERE project_id = ? AND agent_type = ?`)
    .get(projectId, agentType) as { id: string } | undefined
  if (existing) {
    db.prepare(`UPDATE agent_sessions SET claude_session_id = ?, updated_at = ? WHERE id = ?`).run(claudeSessionId, now, existing.id)
  } else {
    db.prepare(
      `INSERT INTO agent_sessions (id, project_id, agent_type, claude_session_id, created_at, updated_at) VALUES (?, ?, ?, ?, ?, ?)`,
    ).run(newId(), projectId, agentType, claudeSessionId, now, now)
  }
}
