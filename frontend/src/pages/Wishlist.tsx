import { Heart, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { EmptyState } from '@/components/common/EmptyState'
import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useToggleWishlist, useWishlist } from '@/hooks/useWishlist'
import { useAddCartItem } from '@/hooks/useCart'
import { formatCurrency } from '@/lib/currency'

export function WishlistPage() {
  const { t } = useTranslation()
  const { data: wishlist, isLoading } = useWishlist()
  const toggleWishlist = useToggleWishlist()
  const addItem = useAddCartItem()

  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
        <Skeleton className="mb-8 h-8 w-40" />
        <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 4 }, (_, i) => (
            <div key={i} className="flex flex-col gap-2">
              <Skeleton className="aspect-square rounded-lg" />
              <Skeleton className="h-4 w-2/3" />
            </div>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
      <h1 className="mb-8 font-serif text-3xl">{t('wishlist.title')}</h1>
      {(!wishlist || wishlist.items.length === 0) && <EmptyState icon={Heart} title={t('wishlist.empty')} />}
      <ul className="grid grid-cols-2 gap-6 sm:grid-cols-3 lg:grid-cols-4">
        {wishlist?.items.map((item) => (
          <li key={item.productId} className="group relative">
            <button
              onClick={() => toggleWishlist.mutate({ productId: item.productId, isWishlisted: true })}
              aria-label={t('common.remove')}
              className="absolute top-2 right-2 z-10 flex size-7 items-center justify-center rounded-full bg-background/80 backdrop-blur transition-colors hover:bg-background"
            >
              <X className="size-3.5" />
            </button>
            <Link
              to={`/products/${item.slug}`}
              className="block aspect-square overflow-hidden rounded-lg bg-muted shadow-sm transition-shadow group-hover:shadow-md"
            >
              <SafeImage src={item.imageUrl} alt={item.name} fallbackLabel={item.name} />
            </Link>
            <div className="mt-2 flex flex-col gap-1">
              <Link to={`/products/${item.slug}`} className="text-sm font-medium hover:underline">
                {item.name}
              </Link>
              <span className="font-serif">{formatCurrency(item.finalPrice, 'AMD')}</span>
              <Button
                size="sm"
                variant="outline"
                disabled={!item.inStock}
                onClick={() => addItem.mutate({ productId: item.productId, quantity: 1 })}
              >
                {item.inStock ? t('product.addToCart') : t('product.outOfStock')}
              </Button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}
