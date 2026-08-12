import type { CommandHandler } from './types.js'
import * as approvalService from '../services/approvalService.js'

export const approveCommand: CommandHandler = async (ctx) => {
  const id = ctx.args.trim()
  if (!id) {
    await ctx.reply('Usage: /approve <approval-id>')
    return
  }
  const resolved = approvalService.resolveApproval(id, 'approved', 'operator')
  await ctx.reply(resolved ? `✅ Approved ${id}.` : `Could not resolve ${id} (already resolved, unknown, or the process restarted while it was pending - use /retry on the affected task instead).`)
}
