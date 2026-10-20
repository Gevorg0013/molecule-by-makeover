import { Loader2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'

import { useLoadingTask } from '@/lib/loadingTasks'
import { cn } from '@/lib/utils'

export function Spinner({ className }: { className?: string }) {
  return (
    <Loader2
      aria-hidden="true"
      className={cn('size-5 animate-spin text-accent motion-reduce:[animation-duration:1.5s]', className)}
    />
  )
}

/**
 * Spinner plus label for a region waiting on data. `page` centres it in a tall block for
 * storefront pages; the default is a compact inline row for admin panels.
 */
export function LoadingState({ variant = 'inline', className }: { variant?: 'inline' | 'page'; className?: string }) {
  const { t } = useTranslation()
  return (
    <div
      role="status"
      aria-live="polite"
      className={cn(
        'flex items-center gap-2 text-sm text-muted-foreground',
        variant === 'page' && 'justify-center px-4 py-20',
        className,
      )}
    >
      <Spinner className={variant === 'page' ? 'size-6' : undefined} />
      <span>{t('common.loading')}</span>
    </div>
  )
}

/** Suspense fallback for lazy routes: shows the spinner and keeps the top progress bar running. */
export function RouteLoadingFallback() {
  useLoadingTask()
  return <LoadingState />
}

/** Placeholder for route guards waiting on auth bootstrap; holds the progress bar open meanwhile. */
export function PendingRoute() {
  useLoadingTask()
  return null
}
