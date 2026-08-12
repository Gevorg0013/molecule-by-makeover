import * as taskService from '../services/taskService.js'
import * as agentOrchestrator from './agentOrchestrator.js'
import type { TaskExecutionCallbacks } from './agentOrchestrator.js'

export interface QueueCallbacks extends TaskExecutionCallbacks {
  onSubtaskStarted?: (title: string, agentType: string) => void
  onSubtaskFinished?: (title: string, agentType: string, success: boolean) => void
}

interface ProjectRunState {
  paused: boolean
  cancelled: boolean
  running: boolean
  controller: AbortController | null
}

const states = new Map<string, ProjectRunState>()

function getState(projectId: string): ProjectRunState {
  let state = states.get(projectId)
  if (!state) {
    state = { paused: false, cancelled: false, running: false, controller: null }
    states.set(projectId, state)
  }
  return state
}

export function isRunning(projectId: string): boolean {
  return getState(projectId).running
}

/** Marks a project as actively worked on (planning/executing/reviewing), independent of runProjectQueue's own scope, so /status is accurate during every phase of startNewProject, not just subtask execution. */
export function setRunning(projectId: string, running: boolean): void {
  getState(projectId).running = running
}

export function isPaused(projectId: string): boolean {
  return getState(projectId).paused
}

export function isCancelled(projectId: string): boolean {
  return getState(projectId).cancelled
}

export function pauseProject(projectId: string): void {
  getState(projectId).paused = true
}

export function resumeProject(projectId: string): void {
  getState(projectId).paused = false
}

/** Stops the queue after the current subtask AND aborts whichever agent run is in flight right now, so /cancel actually interrupts a stuck or long-running call instead of only taking effect between subtasks. */
export function cancelProject(projectId: string): void {
  const state = getState(projectId)
  state.cancelled = true
  state.paused = false
  state.controller?.abort()
}

/** Clears a prior cancellation so a project that was cancelled can be worked on again (e.g. via /retry). */
export function clearCancellation(projectId: string): void {
  getState(projectId).cancelled = false
}

/** Registers a fresh AbortController as "the currently in-flight agent call" for this project, so cancelProject() has something to abort. Call endAbortable() when that call finishes. */
export function beginAbortable(projectId: string): AbortController {
  const state = getState(projectId)
  const controller = new AbortController()
  state.controller = controller
  return controller
}

export function endAbortable(projectId: string): void {
  getState(projectId).controller = null
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

// --- Global run queue ---------------------------------------------------
// This PM has a single operator, so distinct project kickoffs (/new, /retry)
// run one at a time rather than concurrently racing the same working
// directory. Per-project pause/resume/cancel above is orthogonal to this.

let tail: Promise<void> = Promise.resolve()
const pendingLabels: string[] = []

export function queuedRunCount(): number {
  return pendingLabels.length
}

export function enqueueGlobalRun<T>(label: string, run: () => Promise<T>): Promise<T> {
  pendingLabels.push(label)
  const started: Promise<T> = tail.catch(() => {}).then(() => {
    pendingLabels.shift()
    return run()
  })
  tail = started.then(
    () => undefined,
    () => undefined,
  )
  return started
}

/**
 * Runs a project's subtasks in order, one at a time, honoring pause/resume/cancel
 * between tasks. Each task is retried internally by agentOrchestrator up to its
 * own maxAttempts before being counted as failed here. Caller owns the overall
 * `running` flag (see setRunning) since this is only one phase of a project run.
 */
export async function runProjectQueue(
  projectId: string,
  taskIds: string[],
  callbacks: QueueCallbacks,
): Promise<{ completed: number; failed: number; cancelled: boolean }> {
  const state = getState(projectId)
  let completed = 0
  let failed = 0

  for (const taskId of taskIds) {
    while (state.paused && !state.cancelled) {
      await sleep(2000)
    }
    if (state.cancelled) {
      taskService.setStatus(taskId, 'cancelled')
      continue
    }

    const task = taskService.getTask(taskId)
    callbacks.onSubtaskStarted?.(task?.title ?? taskId, task?.agentType ?? 'unknown')
    const controller = beginAbortable(projectId)
    const result = await agentOrchestrator.executeTask(taskId, callbacks, undefined, controller)
    endAbortable(projectId)
    callbacks.onSubtaskFinished?.(task?.title ?? taskId, task?.agentType ?? 'unknown', result.success)
    if (result.success) completed++
    else failed++
  }

  return { completed, failed, cancelled: state.cancelled }
}
