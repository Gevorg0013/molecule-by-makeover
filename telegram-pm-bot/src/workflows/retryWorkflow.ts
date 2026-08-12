import * as agentOrchestrator from '../orchestrator/agentOrchestrator.js'
import * as taskManager from '../orchestrator/taskManager.js'
import * as taskService from '../services/taskService.js'
import * as projectService from '../services/projectService.js'
import type { TaskExecutionCallbacks, TaskExecutionResult } from '../orchestrator/agentOrchestrator.js'

/** Manually retries one task (used by /retry), independent of the original project queue run. */
export async function retryTask(taskId: string, callbacks: TaskExecutionCallbacks): Promise<TaskExecutionResult> {
  const task = taskService.getTask(taskId)
  if (!task) throw new Error(`Task ${taskId} not found`)

  agentOrchestrator.resetForRetry(taskId)
  taskManager.clearCancellation(task.projectId)
  taskManager.setRunning(task.projectId, true)
  try {
    const controller = taskManager.beginAbortable(task.projectId)
    const result = await agentOrchestrator.executeTask(taskId, callbacks, undefined, controller)
    taskManager.endAbortable(task.projectId)

    if (result.success) {
      const project = projectService.getProject(task.projectId)
      if (project?.status === 'needs_review' || project?.status === 'failed') {
        const siblings = taskService.listSubtasks(task.parentTaskId ?? task.id)
        const stillFailing = siblings.some((t) => t.status === 'failed')
        if (!stillFailing) projectService.setStatus(task.projectId, 'completed')
      }
    }

    return result
  } finally {
    taskManager.setRunning(task.projectId, false)
  }
}

export function findMostRecentFailedTask(projectId?: string) {
  const tasks = projectId ? taskService.listTasksForProject(projectId) : taskService.listRecentTasks(50)
  return tasks
    .filter((t) => t.status === 'failed')
    .sort((a, b) => (a.updatedAt < b.updatedAt ? 1 : -1))[0]
}
