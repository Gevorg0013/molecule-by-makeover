import type { OutputFormat } from '@anthropic-ai/claude-agent-sdk'
import { runAgent, type AgentRunCallbacks } from '../agents/agentExecutor.js'
import * as taskService from '../services/taskService.js'
import * as agentRunService from '../services/agentRunService.js'
import * as projectService from '../services/projectService.js'
import * as memoryService from '../memory/memoryService.js'
import { log } from '../services/logService.js'

export interface TaskExecutionCallbacks extends AgentRunCallbacks {
  onAttemptFailed?: (attempt: number, maxAttempts: number, error: string) => void
}

export interface TaskExecutionResult {
  success: boolean
  summary?: string
  structuredOutput?: unknown
  error?: string
}

/**
 * Runs a single task through its assigned agent, retrying automatically
 * (isolated Claude Agent SDK run each attempt, same resumed agent session)
 * until it succeeds or exhausts maxAttempts. `outputFormat` is passed through
 * for callers (e.g. the Planner step) that need structured JSON back.
 */
export async function executeTask(
  taskId: string,
  callbacks: TaskExecutionCallbacks,
  outputFormat?: OutputFormat,
  abortController?: AbortController,
): Promise<TaskExecutionResult> {
  let task = taskService.getTask(taskId)
  if (!task) throw new Error(`Task ${taskId} not found`)

  const project = projectService.getProject(task.projectId)
  taskService.setStatus(task.id, 'running')
  log('info', `Starting task "${task.title}" (${task.agentType})`, { projectId: task.projectId, taskId: task.id })

  let lastError = 'unknown error'

  while (task.attemptCount < task.maxAttempts) {
    taskService.incrementAttempt(task.id)
    task = taskService.getTask(task.id) ?? task

    const run = agentRunService.startRun(task.id, task.agentType)
    const prompt = buildPrompt(task, project?.description)

    const result = await runAgent(task.agentType, task.projectId, task.id, prompt, callbacks, outputFormat, abortController)

    if (result.success) {
      agentRunService.finishRun(run.id, {
        status: 'success',
        claudeSessionId: result.sessionId,
        costUsd: result.costUsd,
        numTurns: result.numTurns,
        summary: result.summary,
      })
      taskService.setStatus(task.id, 'success', { resultSummary: result.summary })
      if (result.summary) {
        memoryService.remember(task.projectId, `task:${task.title}`.slice(0, 120), result.summary.slice(0, 800), task.id)
      }
      log('info', `Task succeeded: ${task.title}`, { projectId: task.projectId, taskId: task.id })
      return { success: true, summary: result.summary, structuredOutput: result.structuredOutput }
    }

    lastError = result.error ?? 'unknown error'
    agentRunService.finishRun(run.id, {
      status: 'error',
      claudeSessionId: result.sessionId,
      costUsd: result.costUsd,
      numTurns: result.numTurns,
      error: lastError,
    })

    if (result.cancelled) {
      taskService.setStatus(task.id, 'cancelled', { error: lastError })
      log('warn', `Task cancelled: ${task.title}`, { projectId: task.projectId, taskId: task.id })
      return { success: false, error: lastError }
    }

    log('warn', `Task attempt ${task.attemptCount}/${task.maxAttempts} failed: ${lastError}`, {
      projectId: task.projectId,
      taskId: task.id,
    })
    callbacks.onAttemptFailed?.(task.attemptCount, task.maxAttempts, lastError)
  }

  taskService.setStatus(task.id, 'failed', { error: lastError })
  log('error', `Task failed after ${task.maxAttempts} attempts: ${task.title}`, { projectId: task.projectId, taskId: task.id })
  return { success: false, error: lastError }
}

/** Resets a failed/cancelled task's attempt counter so it gets a fresh retry budget. */
export function resetForRetry(taskId: string): void {
  taskService.resetAttempts(taskId)
  taskService.setStatus(taskId, 'pending')
}

function buildPrompt(task: { projectId: string; title: string; description: string }, projectDescription?: string): string {
  const parts: string[] = []
  if (projectDescription) parts.push(`Overall project goal: ${projectDescription}`)
  const memoryContext = memoryService.renderContext(task.projectId)
  if (memoryContext) parts.push(memoryContext)
  parts.push(`Your subtask: ${task.title}\n${task.description}`)
  return parts.join('\n\n')
}
