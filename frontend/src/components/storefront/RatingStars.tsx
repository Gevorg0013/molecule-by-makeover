import { Star } from 'lucide-react'

import { cn } from '@/lib/utils'

export function RatingStars({ rating, size = 14 }: { rating: number; size?: number }) {
  return (
    <div className="inline-flex items-center gap-0.5">
      {[1, 2, 3, 4, 5].map((n) => (
        <Star
          key={n}
          size={size}
          className={cn(
            n <= Math.round(rating) ? 'fill-accent text-accent' : 'fill-none text-muted-foreground',
          )}
        />
      ))}
    </div>
  )
}
