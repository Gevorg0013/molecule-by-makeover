import type { CommandHandler } from './types.js'
import * as approvalService from '../services/approvalService.js'

export const rejectCommand: CommandHandler = async (ctx) => {
  const id = ctx.args.trim()
  if (!id) {
    await ctx.reply('Usage: /reject <approval-id>')
    return
  }
  const resolved = approvalService.resolveApproval(id, 'rejected', 'operator')
  await ctx.reply(resolved ? `🚫 Rejected ${id}.` : `Could not resolve ${id} (already resolved, unknown, or the process restarted while it was pending).`)
}
