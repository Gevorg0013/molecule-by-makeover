import { Heart, ShoppingBag } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'

import { RatingStars } from './RatingStars'
import { SafeImage } from '@/components/common/SafeImage'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { useAddCartItem } from '@/hooks/useCart'
import { useToggleWishlist, useWishlist } from '@/hooks/useWishlist'
import { useAuthStore } from '@/store/authStore'
import { formatCurrency } from '@/lib/currency'
import { DiscountType } from '@/types/enums'
import type { ProductListItemDto } from '@/types/dto'

export function ProductCard({ product, currency = 'AMD' }: { product: ProductListItemDto; currency?: string }) {
  const { t } = useTranslation()
  const isAuthed = !!useAuthStore((s) => s.accessToken)
  const addItem = useAddCartItem()
  const toggleWishlist = useToggleWishlist()
  const { data: wishlist } = useWishlist()
  const isWishlisted = !!wishlist?.items.some((i) => i.productId === product.id)
  const hasDiscount = product.discountType !== DiscountType.None && product.finalPrice < product.price

  return (
    <div className="group relative flex flex-col transition-transform duration-300 hover:-translate-y-0.5">
      <Link
        to={`/products/${product.slug}`}
        className="relative block aspect-square overflow-hidden rounded-lg bg-muted shadow-sm transition-shadow duration-300 group-hover:shadow-md"
      >
        <SafeImage
          src={product.mainImageUrl}
          alt={product.name}
          fallbackLabel={product.name}
          className="transition-transform duration-500 group-hover:scale-105"
        />
        {product.stock === 0 && (
          <Badge variant="secondary" className="absolute top-2 left-2">
            {t('product.outOfStock')}
          </Badge>
        )}
        {hasDiscount && product.stock > 0 && (
          <Badge variant="accent" className="absolute top-2 left-2">
            -
            {product.discountType === DiscountType.Percent
              ? `${product.discountValue}%`
              : formatCurrency(product.discountValue ?? 0, currency)}
          </Badge>
        )}
        {isAuthed && (
          <button
            type="button"
            aria-label={t('product.addedToWishlist')}
            onClick={(e) => {
              e.preventDefault()
              toggleWishlist.mutate({ productId: product.id, isWishlisted })
            }}
            className="absolute top-2 right-2 flex size-8 items-center justify-center rounded-full bg-background/80 backdrop-blur transition-colors hover:bg-background"
          >
            <Heart className={cnHeart(isWishlisted)} />
          </button>
        )}
      </Link>

      <div className="mt-3 flex flex-1 flex-col gap-1">
        <Link to={`/products/${product.slug}`} className="text-sm font-medium hover:underline">
          {product.name}
        </Link>
        {product.brand && <p className="text-xs text-muted-foreground">{product.brand}</p>}
        <div className="flex items-center gap-1.5">
          <RatingStars rating={product.averageRating} />
          {product.reviewCount > 0 && (
            <span className="text-xs text-muted-foreground">({product.reviewCount})</span>
          )}
        </div>
        <div className="mt-1 flex items-center gap-2">
          <span className="font-serif text-base">{formatCurrency(product.finalPrice, currency)}</span>
          {hasDiscount && (
            <span className="text-sm text-muted-foreground line-through">
              {formatCurrency(product.price, currency)}
            </span>
          )}
        </div>
      </div>

      <Button
        size="sm"
        variant="outline"
        disabled={product.stock === 0 || addItem.isPending}
        className="mt-3 w-full gap-2"
        onClick={() => {
          addItem.mutate(
            { productId: product.id, quantity: 1 },
            { onSuccess: () => toast.success(t('product.addedToCart')) },
          )
        }}
      >
        <ShoppingBag className="size-4" />
        {product.stock === 0
          ? t('product.outOfStock')
          : addItem.isPending
            ? t('product.addingToCart')
            : t('product.addToCart')}
      </Button>
    </div>
  )
}

function cnHeart(active: boolean) {
  return active ? 'size-4 fill-accent text-accent' : 'size-4 text-foreground'
}
