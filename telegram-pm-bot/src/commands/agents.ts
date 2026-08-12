import type { CommandHandler } from './types.js'
import { AGENT_DEFINITIONS } from '../agents/definitions.js'

export const agentsCommand: CommandHandler = async (ctx) => {
  const lines = Object.entries(AGENT_DEFINITIONS).map(([key, def]) => `• ${def.displayName} (${key}): ${def.description}`)
  await ctx.reply(`Agent roster:\n${lines.join('\n')}`)
}
