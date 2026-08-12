import * as taskRepository from '../database/repositories/taskRepository.js'
import type { CreateTaskInput } from '../database/repositories/taskRepository.js'
import type { Task, TaskStatus } from '../database/types.js'

export function createTask(input: CreateTaskInput): Task {
  return taskRepository.createTask(input)
}

export function getTask(id: string): Task | undefined {
  return taskRepository.getTask(id)
}

export function listTasksForProject(projectId: string): Task[] {
  return taskRepository.listTasksForProject(projectId)
}

export function listSubtasks(parentTaskId: string): Task[] {
  return taskRepository.listSubtasks(parentTaskId)
}

export function listRecentTasks(limit = 10): Task[] {
  return taskRepository.listRecentTasks(limit)
}

export function setStatus(id: string, status: TaskStatus, extra?: { resultSummary?: string; error?: string }): void {
  taskRepository.updateTaskStatus(id, status, extra)
}

export function incrementAttempt(id: string): void {
  taskRepository.incrementAttempt(id)
}

export function resetAttempts(id: string): void {
  taskRepository.resetAttempts(id)
}

export function resetOrphanedRunning(): number {
  return taskRepository.resetOrphanedRunning()
}
