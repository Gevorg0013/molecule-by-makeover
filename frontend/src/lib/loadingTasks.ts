import { useEffect, useSyncExternalStore } from 'react'

// Counts work that isn't a TanStack query but should still hold the top progress bar open:
// lazy route chunks (Suspense fallbacks) and the initial auth bootstrap behind route guards.
let pending = 0
const listeners = new Set<() => void>()

function emit() {
  for (const listener of listeners) listener()
}

function subscribe(listener: () => void) {
  listeners.add(listener)
  return () => listeners.delete(listener)
}

export function usePendingLoadingTasks() {
  return useSyncExternalStore(subscribe, () => pending)
}

/** Registers a pending task for as long as the calling component is mounted and `active` is true. */
export function useLoadingTask(active = true) {
  useEffect(() => {
    if (!active) return
    pending += 1
    emit()
    return () => {
      pending -= 1
      emit()
    }
  }, [active])
}
