import { ImageOff } from 'lucide-react'
import { useState } from 'react'

import { cn } from '@/lib/utils'
import { resolveMediaUrl } from '@/lib/media'

interface SafeImageProps {
  src: string | null | undefined
  alt: string
  className?: string
  /** Eager, high-priority load (use for the single above-the-fold hero image only). */
  priority?: boolean
  fallbackLabel?: string
}

export function SafeImage({ src: rawSrc, alt, className, priority = false, fallbackLabel }: SafeImageProps) {
  const [failed, setFailed] = useState(false)
  const src = resolveMediaUrl(rawSrc)
  const showFallback = !src || failed

  if (showFallback) {
    return (
      <div
        className={cn(
          'flex size-full flex-col items-center justify-center gap-1.5 bg-gradient-to-br from-secondary to-muted text-muted-foreground',
          className,
        )}
      >
        <ImageOff className="size-6 opacity-50" />
        {fallbackLabel && <span className="max-w-[80%] truncate text-center text-xs">{fallbackLabel}</span>}
      </div>
    )
  }

  return (
    <img
      src={src}
      alt={alt}
      className={cn('size-full object-cover', className)}
      loading={priority ? 'eager' : 'lazy'}
      fetchPriority={priority ? 'high' : 'auto'}
      decoding={priority ? 'sync' : 'async'}
      onError={() => setFailed(true)}
    />
  )
}
