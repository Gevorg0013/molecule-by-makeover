import fs from 'node:fs'
import path from 'node:path'
import { config } from '../config/env.js'
import type { AgentType } from '../database/types.js'

/**
 * Backend/Frontend agents are scoped to their conventional subdirectory when
 * one exists (matches this repo's backend/ + frontend/ layout), otherwise
 * they fall back to the repo root so the bot still works on projects without
 * that split.
 */
export function cwdForAgent(agentType: AgentType): string {
  if (agentType === 'backend') return subdirIfExists('backend')
  if (agentType === 'frontend') return subdirIfExists('frontend')
  return config.workingDir
}

function subdirIfExists(name: string): string {
  const candidate = path.join(config.workingDir, name)
  try {
    return fs.statSync(candidate).isDirectory() ? candidate : config.workingDir
  } catch {
    return config.workingDir
  }
}
