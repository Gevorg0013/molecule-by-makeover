import { useIsFetching } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useLocation, useNavigation } from 'react-router-dom'

import { usePendingLoadingTasks } from '@/lib/loadingTasks'
import { cn } from '@/lib/utils'

// Transitions that finish within this window never show the bar, so cached pages don't flash it.
const SHOW_DELAY_MS = 150
// After a route change, wait this long with nothing in flight before calling it done. Gives the
// new page's queries time to subscribe; without it the bar would finish before they start.
const SETTLE_MS = 150
// Hard ceiling per route so a hung request or chunk can never leave the bar stuck on screen.
const MAX_DURATION_MS = 15_000
const TRICKLE_MS = 250
const FINISH_MS = 400

type Phase = 'idle' | 'running' | 'finishing'

/**
 * Thin bar pinned to the top of the viewport. A route counts as loading from the moment its
 * pathname/search changes (link, back/forward, direct load or refresh) until no TanStack query
 * is fetching and no lazy chunk or auth bootstrap is pending. Background refetches outside a
 * route change don't trigger it.
 */
export function RouteProgress() {
  const location = useLocation()
  const navigation = useNavigation()
  const isFetching = useIsFetching()
  const pendingTasks = usePendingLoadingTasks()

  // Hash-only changes (in-page anchors) and history state updates aren't page transitions.
  const routeKey = location.pathname + location.search
  const [settledKey, setSettledKey] = useState<string | null>(null)
  const [expiredKey, setExpiredKey] = useState<string | null>(null)

  const routeLoading = settledKey !== routeKey
  const busy = routeLoading || navigation.state !== 'idle' || pendingTasks > 0
  const active = busy && expiredKey !== routeKey

  useEffect(() => {
    if (!routeLoading || isFetching > 0 || pendingTasks > 0) return
    const id = setTimeout(() => setSettledKey(routeKey), SETTLE_MS)
    return () => clearTimeout(id)
  }, [routeLoading, isFetching, pendingTasks, routeKey])

  useEffect(() => {
    const id = setTimeout(() => setExpiredKey(routeKey), MAX_DURATION_MS)
    return () => clearTimeout(id)
  }, [routeKey])

  // Starts 'running' so a direct load/refresh carries on from the static bar in index.html.
  const [phase, setPhase] = useState<Phase>('running')
  const [progress, setProgress] = useState(0.3)

  useEffect(() => {
    if (active) {
      if (phase === 'running') {
        // Ease towards 90% and never reach it; only completion takes the bar to 100%.
        const id = setInterval(() => setProgress((p) => p + (0.9 - p) * 0.1), TRICKLE_MS)
        return () => clearInterval(id)
      }
      const id = setTimeout(
        () => {
          setProgress(0.1)
          setPhase('running')
        },
        phase === 'idle' ? SHOW_DELAY_MS : 0,
      )
      return () => clearTimeout(id)
    }
    if (phase === 'running') {
      const id = setTimeout(() => {
        setProgress(1)
        setPhase('finishing')
      }, 0)
      return () => clearTimeout(id)
    }
    if (phase === 'finishing') {
      const id = setTimeout(() => setPhase('idle'), FINISH_MS)
      return () => clearTimeout(id)
    }
  }, [active, phase])

  // Unmounted while idle so the next run starts from the left instead of animating backwards.
  if (phase === 'idle') return null

  return (
    <div aria-hidden="true" className="pointer-events-none fixed inset-x-0 top-0 z-[100] h-0.5 sm:h-[3px]">
      <div
        className={cn(
          'h-full origin-left bg-accent shadow-[0_0_8px_var(--accent)]',
          phase === 'finishing' && 'opacity-0',
        )}
        style={{
          transform: `scaleX(${progress})`,
          // Fill to 100% first, then fade, so completion is visible before the bar goes.
          transition: 'transform 200ms ease-out, opacity 200ms ease-out 150ms',
        }}
      />
    </div>
  )
}
