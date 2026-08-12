import type { OutputFormat } from '@anthropic-ai/claude-agent-sdk'
import * as projectService from '../services/projectService.js'
import * as taskService from '../services/taskService.js'
import * as agentOrchestrator from '../orchestrator/agentOrchestrator.js'
import * as taskManager from '../orchestrator/taskManager.js'
import type { QueueCallbacks } from '../orchestrator/taskManager.js'
import { config } from '../config/env.js'
import type { AgentType, Project, Task } from '../database/types.js'

const AGENT_TYPES: AgentType[] = ['backend', 'frontend', 'qa', 'research', 'reviewer', 'devops']

const PLAN_OUTPUT_FORMAT: OutputFormat = {
  type: 'json_schema',
  schema: {
    type: 'object',
    properties: {
      subtasks: {
        type: 'array',
        items: {
          type: 'object',
          properties: {
            agent: { type: 'string', enum: AGENT_TYPES },
            title: { type: 'string' },
            description: { type: 'string' },
          },
          required: ['agent', 'title', 'description'],
          additionalProperties: false,
        },
      },
    },
    required: ['subtasks'],
    additionalProperties: false,
  },
}

interface PlanSubtask {
  agent: AgentType
  title: string
  description: string
}

interface Plan {
  subtasks: PlanSubtask[]
}

export interface NewProjectCallbacks extends QueueCallbacks {
  onPhase?: (phase: 'planning' | 'executing' | 'reviewing') => void
  onPlanReady?: (subtasks: PlanSubtask[]) => void
}

export interface NewProjectResult {
  project: Project
  rootTask: Task
  ok: boolean
  reason?: string
  completed?: number
  failed?: number
  reviewSummary?: string
}

function isValidPlan(value: unknown): value is Plan {
  if (!value || typeof value !== 'object') return false
  const subtasks = (value as { subtasks?: unknown }).subtasks
  if (!Array.isArray(subtasks) || subtasks.length === 0) return false
  return subtasks.every(
    (s) =>
      s &&
      typeof s === 'object' &&
      typeof (s as PlanSubtask).title === 'string' &&
      typeof (s as PlanSubtask).description === 'string' &&
      AGENT_TYPES.includes((s as PlanSubtask).agent),
  )
}

export async function startNewProject(name: string, description: string, callbacks: NewProjectCallbacks): Promise<NewProjectResult> {
  const project = projectService.createProject(name, description)
  const rootTask = taskService.createTask({
    projectId: project.id,
    parentTaskId: null,
    title: `Project: ${name}`,
    description,
    agentType: 'planner',
    orderIndex: 0,
    maxAttempts: config.maxAttemptsPerTask,
  })

  taskManager.setRunning(project.id, true)
  try {
    callbacks.onPhase?.('planning')
    const planController = taskManager.beginAbortable(project.id)
    const planResult = await agentOrchestrator.executeTask(rootTask.id, callbacks, PLAN_OUTPUT_FORMAT, planController)
    taskManager.endAbortable(project.id)

    if (taskManager.isCancelled(project.id)) {
      projectService.setStatus(project.id, 'cancelled')
      taskService.setStatus(rootTask.id, 'cancelled')
      return { project, rootTask, ok: false, reason: 'Cancelled by operator.' }
    }

    if (!planResult.success || !isValidPlan(planResult.structuredOutput)) {
      projectService.setStatus(project.id, 'failed')
      return { project, rootTask, ok: false, reason: planResult.error ?? 'Planner did not return a usable subtask plan.' }
    }

    const plan = planResult.structuredOutput
    const subtaskIds = plan.subtasks.map((s, index) => {
      const task = taskService.createTask({
        projectId: project.id,
        parentTaskId: rootTask.id,
        title: s.title,
        description: s.description,
        agentType: s.agent,
        orderIndex: index + 1,
        maxAttempts: config.maxAttemptsPerTask,
      })
      return task.id
    })
    callbacks.onPlanReady?.(plan.subtasks)

    projectService.setStatus(project.id, 'running')
    callbacks.onPhase?.('executing')

    const { completed, failed, cancelled } = await taskManager.runProjectQueue(project.id, subtaskIds, callbacks)

    if (cancelled) {
      projectService.setStatus(project.id, 'cancelled')
      taskService.setStatus(rootTask.id, 'cancelled')
      return { project, rootTask, ok: false, reason: 'Cancelled by operator.', completed, failed }
    }

    callbacks.onPhase?.('reviewing')
    const reviewTask = taskService.createTask({
      projectId: project.id,
      parentTaskId: rootTask.id,
      title: 'Final review',
      description: `Run \`git status\` and \`git diff\` to see everything changed for this project, and review it against the original goal:\n\n${description}\n\nReport whether it looks ready, and flag anything that needs follow-up.`,
      agentType: 'reviewer',
      orderIndex: plan.subtasks.length + 1,
      maxAttempts: 1,
    })
    const reviewController = taskManager.beginAbortable(project.id)
    const reviewResult = await agentOrchestrator.executeTask(reviewTask.id, callbacks, undefined, reviewController)
    taskManager.endAbortable(project.id)

    if (taskManager.isCancelled(project.id)) {
      projectService.setStatus(project.id, 'cancelled')
      taskService.setStatus(rootTask.id, 'cancelled')
      return { project, rootTask, ok: false, reason: 'Cancelled by operator.', completed, failed }
    }

    const finalStatus = failed > 0 ? 'needs_review' : 'completed'
    projectService.setStatus(project.id, finalStatus)
    taskService.setStatus(rootTask.id, failed > 0 ? 'failed' : 'success', { resultSummary: reviewResult.summary })

    return { project, rootTask, ok: failed === 0, completed, failed, reviewSummary: reviewResult.summary }
  } finally {
    taskManager.setRunning(project.id, false)
  }
}
