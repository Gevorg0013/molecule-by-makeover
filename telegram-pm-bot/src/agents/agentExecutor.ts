import { AbortError, query, type CanUseTool, type OutputFormat, type SDKMessage, type SDKResultMessage } from '@anthropic-ai/claude-agent-sdk'
import { config } from '../config/env.js'
import type { AgentType } from '../database/types.js'
import type { Approval } from '../database/types.js'
import * as agentSessionRepository from '../database/repositories/agentSessionRepository.js'
import { requestApproval } from '../services/approvalService.js'
import { isDestructiveCommand } from '../utils/destructiveCommand.js'
import { cwdForAgent } from '../utils/paths.js'
import { AGENT_DEFINITIONS } from './definitions.js'

export interface AgentRunCallbacks {
  onText?: (text: string) => void
  onToolUse?: (toolName: string, input: Record<string, unknown>) => void
  onApprovalNeeded?: (approval: Approval) => void
}

export interface AgentRunResult {
  success: boolean
  sessionId?: string
  costUsd?: number
  numTurns?: number
  summary?: string
  structuredOutput?: unknown
  error?: string
  cancelled?: boolean
}

export async function runAgent(
  agentType: AgentType,
  projectId: string,
  taskId: string,
  prompt: string,
  callbacks: AgentRunCallbacks,
  outputFormat?: OutputFormat,
  abortController?: AbortController,
): Promise<AgentRunResult> {
  const definition = AGENT_DEFINITIONS[agentType]
  const resumeSessionId = agentSessionRepository.getAgentSessionId(projectId, agentType)

  const canUseTool: CanUseTool = async (toolName, input, opts) => {
    const command = typeof input.command === 'string' ? input.command : undefined
    if (toolName === 'Bash' && command && isDestructiveCommand(command)) {
      const { approval, wait } = requestApproval({
        projectId,
        taskId,
        description: `${definition.displayName} agent wants to run a potentially destructive command`,
        command,
      })
      callbacks.onApprovalNeeded?.(approval)
      const approved = await raceWithAbort(wait, opts.signal)
      if (approved === null) {
        return { behavior: 'deny', message: 'Cancelled by operator.', interrupt: true }
      }
      if (approved) return { behavior: 'allow', updatedInput: input }
      return { behavior: 'deny', message: 'Operator rejected this command via /reject. Choose a different, non-destructive approach or stop.' }
    }
    return { behavior: 'allow', updatedInput: input }
  }

  const q = query({
    prompt,
    options: {
      cwd: cwdForAgent(agentType),
      model: config.model,
      permissionMode: config.permissionMode,
      canUseTool,
      abortController,
      maxTurns: config.maxTurnsPerAgentRun,
      maxBudgetUsd: config.maxBudgetUsdPerAgentRun,
      resume: resumeSessionId,
      settingSources: ['project'],
      tools: definition.tools,
      outputFormat,
      systemPrompt: {
        type: 'preset',
        preset: 'claude_code',
        append: definition.systemPrompt,
      },
    },
  })

  let finalResult: SDKResultMessage | undefined

  try {
    for await (const message of q as AsyncGenerator<SDKMessage, void>) {
      switch (message.type) {
        case 'assistant':
          for (const block of message.message.content) {
            if (block.type === 'text' && block.text.trim()) {
              callbacks.onText?.(block.text.trim())
            } else if (block.type === 'tool_use') {
              callbacks.onToolUse?.(block.name, block.input as Record<string, unknown>)
            }
          }
          break
        case 'result':
          finalResult = message
          break
        default:
          break
      }
    }
  } catch (err) {
    if (err instanceof AbortError || abortController?.signal.aborted) {
      return { success: false, error: 'Cancelled by operator.', cancelled: true }
    }
    throw err
  }

  if (!finalResult) {
    return { success: false, error: 'Agent run ended without a result message.' }
  }

  agentSessionRepository.setAgentSessionId(projectId, agentType, finalResult.session_id)

  if (finalResult.subtype === 'success') {
    return {
      success: true,
      sessionId: finalResult.session_id,
      costUsd: finalResult.total_cost_usd,
      numTurns: finalResult.num_turns,
      summary: finalResult.result,
      structuredOutput: finalResult.structured_output,
    }
  }

  return {
    success: false,
    sessionId: finalResult.session_id,
    costUsd: finalResult.total_cost_usd,
    numTurns: finalResult.num_turns,
    error: `${finalResult.subtype}: ${finalResult.errors.join('; ')}`,
  }
}

/** Resolves to null if the abort signal fires before the promise settles - unblocks a pending approval wait when /cancel is invoked. */
function raceWithAbort(promise: Promise<boolean>, signal: AbortSignal): Promise<boolean | null> {
  if (signal.aborted) return Promise.resolve(null)
  return new Promise((resolve) => {
    const onAbort = () => resolve(null)
    signal.addEventListener('abort', onAbort, { once: true })
    promise.then((value) => {
      signal.removeEventListener('abort', onAbort)
      resolve(value)
    })
  })
}
