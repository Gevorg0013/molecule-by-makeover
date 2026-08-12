import * as logRepository from '../database/repositories/logRepository.js'
import type { LogLevel } from '../database/types.js'
import { logger } from '../utils/logger.js'

export interface LogContext {
  projectId?: string | null
  taskId?: string | null
  agentRunId?: string | null
}

export function log(level: LogLevel, message: string, ctx: LogContext = {}): void {
  logRepository.addLog({ ...ctx, level, message })
  if (level === 'error') logger.error(message)
  else if (level === 'warn') logger.warn(message)
  else logger.info(message)
}

export function recentLogs(limit = 30, projectId?: string) {
  return logRepository.listRecentLogs(limit, projectId)
}
