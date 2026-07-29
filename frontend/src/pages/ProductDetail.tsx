import { useState } from 'react'
import { Heart, Minus, Plus, ShoppingBag } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { toast } from 'sonner'

import { ProductCard } from '@/components/storefront/ProductCard'
import { RatingStars } from '@/components/storefront/RatingStars'
import { SafeImage } from '@/components/common/SafeImage'
import { ErrorState } from '@/components/common/ErrorState'
import { NotFoundPage } from '@/pages/NotFound'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Textarea } from '@/components/ui/textarea'
import { useAddCartItem } from '@/hooks/useCart'
import { useCreateReview, useReviews } from '@/hooks/useReviews'
import { useProduct, useRelatedProducts } from '@/hooks/useProducts'
import { useToggleWishlist, useWishlist } from '@/hooks/useWishlist'
import { useAuthStore } from '@/store/authStore'
import { formatCurrency } from '@/lib/currency'
import { formatDate } from '@/lib/date'
import { cn } from '@/lib/utils'
import { DiscountType } from '@/types/enums'

export function ProductDetailPage() {
  const { t } = useTranslation()
  const { slug } = useParams()
  const { data: product, isLoading, isError, refetch } = useProduct(slug)
  const { data: related } = useRelatedProducts(product?.id)
  const { data: reviews } = useReviews(product?.id)
  const { data: wishlist } = useWishlist()
  const isAuthed = !!useAuthStore((s) => s.accessToken)
  const addItem = useAddCartItem()
  const toggleWishlist = useToggleWishlist()
  const createReview = useCreateReview(product?.id ?? '')

  const [quantity, setQuantity] = useState(1)
  const [activeImage, setActiveImage] = useState(0)
  const [previewImage, setPreviewImage] = useState<number | null>(null)
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')

  if (isLoading) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
        <div className="grid gap-10 md:grid-cols-2">
          <Skeleton className="aspect-square w-full rounded-lg" />
          <div className="flex flex-col gap-3">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-8 w-2/3" />
            <Skeleton className="h-5 w-32" />
            <Skeleton className="mt-4 h-9 w-40" />
            <Skeleton className="mt-6 h-11 w-full" />
          </div>
        </div>
      </div>
    )
  }
  if (isError) return <ErrorState onRetry={() => refetch()} />
  if (!product) return <NotFoundPage />

  const isWishlisted = !!wishlist?.items.some((i) => i.productId === product.id)
  const hasDiscount = product.discountType !== DiscountType.None && product.finalPrice < product.price
  const images = product.images.length > 0 ? product.images : product.mainImageUrl
    ? [{ id: 'main', url: product.mainImageUrl, altText: product.name, sortOrder: 0, isPrimary: true }]
    : []
  const displayedImage = images[previewImage ?? activeImage]

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <div className="grid gap-10 md:grid-cols-2">
        <div>
          <div className="aspect-square overflow-hidden rounded-lg bg-muted">
            <SafeImage src={displayedImage?.url} alt={product.name} fallbackLabel={product.name} priority />
          </div>
          {images.length > 1 && (
            <div className="mt-3 flex gap-2">
              {images.map((img, i) => (
                <button
                  key={img.id}
                  onClick={() => setActiveImage(i)}
                  onMouseEnter={() => setPreviewImage(i)}
                  onMouseLeave={() => setPreviewImage(null)}
                  className={cn(
                    'size-16 overflow-hidden rounded-md border-2 transition-colors',
                    i === activeImage ? 'border-accent' : 'border-transparent hover:border-muted-foreground/40',
                  )}
                >
                  <SafeImage src={img.url} alt="" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div>
          {product.brand && <p className="text-sm text-muted-foreground">{product.brand}</p>}
          <h1 className="font-serif text-3xl">{product.name}</h1>
          <div className="mt-2 flex items-center gap-2">
            <RatingStars rating={product.averageRating} />
            <span className="text-sm text-muted-foreground">({product.reviewCount})</span>
          </div>
          <div className="mt-4 flex items-center gap-3">
            <span className="font-serif text-2xl">{formatCurrency(product.finalPrice, 'AMD')}</span>
            {hasDiscount && (
              <span className="text-lg text-muted-foreground line-through">
                {formatCurrency(product.price, 'AMD')}
              </span>
            )}
            {product.stock > 0 ? (
              <Badge variant="secondary">{t('product.inStock')}</Badge>
            ) : (
              <Badge variant="destructive">{t('product.outOfStock')}</Badge>
            )}
          </div>
          {product.shortDescription && <p className="mt-4 text-muted-foreground">{product.shortDescription}</p>}

          <div className="mt-6 flex items-center gap-3">
            <div className="flex items-center rounded-md border">
              <button
                className="flex size-9 items-center justify-center disabled:opacity-40"
                disabled={quantity <= 1}
                onClick={() => setQuantity((q) => Math.max(1, q - 1))}
              >
                <Minus className="size-4" />
              </button>
              <span className="w-8 text-center text-sm">{quantity}</span>
              <button
                className="flex size-9 items-center justify-center disabled:opacity-40"
                disabled={quantity >= product.stock}
                onClick={() => setQuantity((q) => Math.min(product.stock, q + 1))}
              >
                <Plus className="size-4" />
              </button>
            </div>
            <Button
              size="lg"
              className="flex-1 gap-2"
              disabled={product.stock === 0 || addItem.isPending}
              onClick={() =>
                addItem.mutate(
                  { productId: product.id, quantity },
                  { onSuccess: () => toast.success(t('product.addedToCart')) },
                )
              }
            >
              <ShoppingBag className="size-4" />
              {t('product.addToCart')}
            </Button>
            {isAuthed && (
              <Button
                size="lg"
                variant="outline"
                onClick={() => toggleWishlist.mutate({ productId: product.id, isWishlisted })}
              >
                <Heart className={isWishlisted ? 'size-4 fill-accent text-accent' : 'size-4'} />
              </Button>
            )}
          </div>

          <Tabs defaultValue="description" className="mt-10">
            <TabsList>
              <TabsTrigger value="description">{t('product.description')}</TabsTrigger>
              {product.ingredients && <TabsTrigger value="ingredients">{t('product.ingredients')}</TabsTrigger>}
              {product.usageInstructions && <TabsTrigger value="usage">{t('product.usage')}</TabsTrigger>}
              {product.benefits && <TabsTrigger value="benefits">{t('product.benefits')}</TabsTrigger>}
            </TabsList>
            <TabsContent value="description" className="pt-4 text-sm text-muted-foreground whitespace-pre-line">
              {product.description}
            </TabsContent>
            {product.ingredients && (
              <TabsContent value="ingredients" className="pt-4 text-sm text-muted-foreground whitespace-pre-line">
                {product.ingredients}
              </TabsContent>
            )}
            {product.usageInstructions && (
              <TabsContent value="usage" className="pt-4 text-sm text-muted-foreground whitespace-pre-line">
                {product.usageInstructions}
              </TabsContent>
            )}
            {product.benefits && (
              <TabsContent value="benefits" className="pt-4 text-sm text-muted-foreground whitespace-pre-line">
                {product.benefits}
              </TabsContent>
            )}
          </Tabs>
        </div>
      </div>

      <Separator className="my-14" />

      <section>
        <h2 className="mb-6 font-serif text-2xl">{t('product.reviews')}</h2>
        <div className="grid gap-10 md:grid-cols-[1fr_320px]">
          <div className="flex flex-col gap-6">
            {reviews && reviews.length === 0 && <p className="text-sm text-muted-foreground">{t('product.noReviews')}</p>}
            {reviews?.map((r) => (
              <div key={r.id} className="border-b pb-4">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{r.reviewerName}</span>
                  <span className="text-xs text-muted-foreground">{formatDate(r.createdAt)}</span>
                </div>
                <RatingStars rating={r.rating} />
                {r.comment && <p className="mt-2 text-sm text-muted-foreground">{r.comment}</p>}
              </div>
            ))}
          </div>

          {isAuthed && (
            <div>
              <h3 className="mb-3 font-medium">{t('product.writeReview')}</h3>
              <form
                className="flex flex-col gap-3"
                onSubmit={(e) => {
                  e.preventDefault()
                  createReview.mutate(
                    { rating, comment },
                    {
                      onSuccess: () => {
                        toast.success(t('product.reviewSubmitted'))
                        setComment('')
                      },
                    },
                  )
                }}
              >
                <div className="flex items-center gap-1">
                  {[1, 2, 3, 4, 5].map((n) => (
                    <button key={n} type="button" onClick={() => setRating(n)}>
                      <span className={n <= rating ? 'text-accent' : 'text-muted-foreground'}>★</span>
                    </button>
                  ))}
                </div>
                <Textarea
                  placeholder={t('product.comment')}
                  value={comment}
                  onChange={(e) => setComment(e.target.value)}
                  rows={4}
                />
                <Button type="submit" disabled={createReview.isPending}>
                  {t('product.submitReview')}
                </Button>
              </form>
            </div>
          )}
        </div>
      </section>

      {related && related.length > 0 && (
        <section className="mt-16">
          <h2 className="mb-6 font-serif text-2xl">{t('product.relatedProducts')}</h2>
          <div className="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 lg:grid-cols-4">
            {related.map((p) => (
              <ProductCard key={p.id} product={p} />
            ))}
          </div>
        </section>
      )}
    </div>
  )
}
