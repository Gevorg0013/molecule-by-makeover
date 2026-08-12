export type AgentType = 'planner' | 'backend' | 'frontend' | 'qa' | 'research' | 'reviewer' | 'devops'

export type ProjectStatus = 'planning' | 'running' | 'paused' | 'needs_review' | 'completed' | 'failed' | 'cancelled'

export type TaskStatus = 'pending' | 'running' | 'blocked_approval' | 'paused' | 'success' | 'failed' | 'cancelled'

export type AgentRunStatus = 'running' | 'success' | 'error'

export type ApprovalStatus = 'pending' | 'approved' | 'rejected'

export type LogLevel = 'info' | 'warn' | 'error'

export interface Project {
  id: string
  name: string
  description: string
  status: ProjectStatus
  createdAt: string
  updatedAt: string
}

export interface Task {
  id: string
  projectId: string
  parentTaskId: string | null
  title: string
  description: string
  agentType: AgentType
  status: TaskStatus
  orderIndex: number
  attemptCount: number
  maxAttempts: number
  resultSummary: string | null
  error: string | null
  createdAt: string
  updatedAt: string
}

export interface AgentRun {
  id: string
  taskId: string
  agentType: AgentType
  claudeSessionId: string | null
  status: AgentRunStatus
  startedAt: string
  finishedAt: string | null
  costUsd: number | null
  numTurns: number | null
  error: string | null
  summary: string | null
}

export interface MemoryEntry {
  id: string
  projectId: string | null
  taskId: string | null
  key: string
  value: string
  createdAt: string
  updatedAt: string
}

export interface LogEntry {
  id: string
  projectId: string | null
  taskId: string | null
  agentRunId: string | null
  level: LogLevel
  message: string
  createdAt: string
}

export interface Approval {
  id: string
  projectId: string | null
  taskId: string | null
  description: string
  command: string | null
  status: ApprovalStatus
  requestedAt: string
  resolvedAt: string | null
  resolvedBy: string | null
}

export interface AgentSession {
  id: string
  projectId: string
  agentType: AgentType
  claudeSessionId: string
  createdAt: string
  updatedAt: string
}
