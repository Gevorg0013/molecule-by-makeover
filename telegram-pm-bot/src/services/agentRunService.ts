import * as agentRunRepository from '../database/repositories/agentRunRepository.js'
import type { AgentRun, AgentRunStatus, AgentType } from '../database/types.js'

export function startRun(taskId: string, agentType: AgentType): AgentRun {
  return agentRunRepository.startAgentRun(taskId, agentType)
}

export function finishRun(
  id: string,
  result: { status: AgentRunStatus; claudeSessionId?: string; costUsd?: number; numTurns?: number; error?: string; summary?: string },
): void {
  agentRunRepository.finishAgentRun(id, result)
}

export function listRecentRuns(limit = 10): AgentRun[] {
  return agentRunRepository.listRecentRuns(limit)
}

export function closeOrphanedRuns(): number {
  return agentRunRepository.closeOrphanedRuns()
}
