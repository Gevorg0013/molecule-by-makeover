import * as memoryRepository from '../database/repositories/memoryRepository.js'

/**
 * Durable project memory: short key/value notes agents and the orchestrator
 * write as they work (decisions, plans, summaries) and read back for context
 * on later tasks in the same project. Not a replacement for the Claude Agent
 * SDK's own per-agent conversation history (see agentSessionRepository) -
 * this is cross-agent, human-readable project knowledge.
 */

export function remember(projectId: string, key: string, value: string, taskId?: string): void {
  memoryRepository.upsertMemory(projectId, key, value, taskId ?? null)
}

export function recall(projectId: string): { key: string; value: string }[] {
  return memoryRepository.listMemoryForProject(projectId).map((m) => ({ key: m.key, value: m.value }))
}

/** Renders memory as a compact context block to prepend to an agent's prompt. */
export function renderContext(projectId: string): string {
  const entries = recall(projectId)
  if (entries.length === 0) return ''
  const lines = entries.map((e) => `- ${e.key}: ${e.value}`)
  return `Project memory (prior decisions/context for this project):\n${lines.join('\n')}`
}
