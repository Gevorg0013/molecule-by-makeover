import * as approvalRepository from '../database/repositories/approvalRepository.js'
import type { Approval } from '../database/types.js'

interface PendingWait {
  resolve: (approved: boolean) => void
}

const pending = new Map<string, PendingWait>()

export function requestApproval(input: {
  projectId?: string | null
  taskId?: string | null
  description: string
  command?: string | null
}): { approval: Approval; wait: Promise<boolean> } {
  const approval = approvalRepository.createApproval(input)
  const wait = new Promise<boolean>((resolve) => {
    pending.set(approval.id, { resolve })
  })
  return { approval, wait }
}

/** Returns false if there was no in-memory waiter for this approval (e.g. process restarted while it was pending). */
export function resolveApproval(id: string, decision: 'approved' | 'rejected', resolvedBy: string): boolean {
  const approval = approvalRepository.getApproval(id)
  if (!approval || approval.status !== 'pending') return false

  approvalRepository.resolveApproval(id, decision, resolvedBy)

  const waiter = pending.get(id)
  if (!waiter) return false
  waiter.resolve(decision === 'approved')
  pending.delete(id)
  return true
}

export function listPending(): Approval[] {
  return approvalRepository.listPendingApprovals()
}
