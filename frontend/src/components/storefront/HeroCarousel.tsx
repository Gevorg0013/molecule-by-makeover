import { ChevronLeft, ChevronRight } from 'lucide-react'
import { type TouchEvent, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'
import type { BannerDto } from '@/types/dto'

const AUTOPLAY_MS = 6000
const SWIPE_THRESHOLD_PX = 50

/**
 * Home-page hero that cycles through every active banner. Autoplay pauses while the pointer or
 * keyboard focus is on it and is off entirely for reduced-motion users; any manual move
 * (arrow, dot, swipe) restarts the timer so the chosen slide stays up for a full interval.
 */
export function HeroCarousel({ banners }: { banners: BannerDto[] }) {
  const { t } = useTranslation()
  const [index, setIndex] = useState(0)
  const [paused, setPaused] = useState(false)
  const touchStartX = useRef<number | null>(null)

  const count = banners.length
  // Banners can be removed between refetches; keep the index in range without a reset effect.
  const current = count > 0 ? index % count : 0
  const banner = banners[current] as BannerDto | undefined

  function goTo(next: number) {
    setIndex(((next % count) + count) % count)
  }

  useEffect(() => {
    if (count < 2 || paused) return
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
    const id = setTimeout(() => setIndex((i) => (i + 1) % count), AUTOPLAY_MS)
    return () => clearTimeout(id)
  }, [current, count, paused])

  function handleTouchStart(e: TouchEvent) {
    touchStartX.current = e.touches[0].clientX
  }

  function handleTouchEnd(e: TouchEvent) {
    if (touchStartX.current === null) return
    const delta = e.changedTouches[0].clientX - touchStartX.current
    touchStartX.current = null
    if (Math.abs(delta) < SWIPE_THRESHOLD_PX) return
    goTo(delta < 0 ? current + 1 : current - 1)
  }

  return (
    <section
      aria-roledescription="carousel"
      className="relative flex min-h-[60vh] items-center justify-center overflow-hidden bg-secondary/50"
      onMouseEnter={() => setPaused(true)}
      onMouseLeave={() => setPaused(false)}
      onFocus={() => setPaused(true)}
      onBlur={() => setPaused(false)}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
    >
      {/* All images stay mounted and cross-fade, so switching never waits on a fresh download. */}
      {banners.map((b, i) => (
        <div
          key={b.id}
          aria-hidden={i !== current}
          className={cn(
            'absolute inset-0 transition-opacity duration-700 ease-in-out motion-reduce:transition-none',
            i === current ? 'opacity-100' : 'opacity-0',
          )}
        >
          <SafeImage src={b.imageUrl} alt={b.title ?? t('brand.name')} priority={i === 0} />
        </div>
      ))}
      <div className="absolute inset-0 bg-black/30" />

      <div key={banner?.id} className="relative z-10 mx-auto max-w-2xl px-12 text-center text-white sm:px-4">
        <h1 className="font-serif text-4xl sm:text-5xl">{banner?.title ?? t('brand.name')}</h1>
        <p className="mt-3 text-lg text-white/90">{banner?.subtitle ?? t('brand.tagline')}</p>
        <Button size="lg" variant="accent" className="mt-6" asChild>
          <Link to={'/products'}>{banner?.ctaText || t('nav.shop')}</Link>
        </Button>
      </div>

      {count > 1 && (
        <>
          <button
            type="button"
            onClick={() => goTo(current - 1)}
            aria-label={t('common.previous')}
            className="absolute left-2 top-1/2 z-10 flex size-10 -translate-y-1/2 items-center justify-center rounded-full bg-black/25 text-white transition-colors hover:bg-black/45 sm:left-4"
          >
            <ChevronLeft className="size-5" />
          </button>
          <button
            type="button"
            onClick={() => goTo(current + 1)}
            aria-label={t('common.next')}
            className="absolute right-2 top-1/2 z-10 flex size-10 -translate-y-1/2 items-center justify-center rounded-full bg-black/25 text-white transition-colors hover:bg-black/45 sm:right-4"
          >
            <ChevronRight className="size-5" />
          </button>
          <div className="absolute inset-x-0 bottom-4 z-10 flex justify-center gap-2">
            {banners.map((b, i) => (
              <button
                key={b.id}
                type="button"
                onClick={() => goTo(i)}
                aria-label={`${i + 1} / ${count}`}
                aria-current={i === current}
                className={cn(
                  'h-1.5 rounded-full transition-all',
                  i === current ? 'w-6 bg-white' : 'w-1.5 bg-white/50 hover:bg-white/80',
                )}
              />
            ))}
          </div>
        </>
      )}
    </section>
  )
}
