import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { HeroCarousel } from '@/components/storefront/HeroCarousel'
import { ProductCard } from '@/components/storefront/ProductCard'
import { ProductGridSkeleton } from '@/components/storefront/ProductCardSkeleton'
import { SafeImage } from '@/components/common/SafeImage'
import { Skeleton } from '@/components/ui/skeleton'
import { useActiveBanners } from '@/hooks/useBanners'
import { useCategories } from '@/hooks/useCategories'
import { useProductList } from '@/hooks/useProducts'

export function HomePage() {
  const { t } = useTranslation()
  const { data: banners, isLoading: bannersLoading } = useActiveBanners()
  const { data: categories, isLoading: categoriesLoading } = useCategories()
  const { data: featured, isLoading: featuredLoading } = useProductList({ featuredOnly: true, pageSize: 8 })
  const { data: bestSellers, isLoading: bestSellersLoading } = useProductList({ bestSellerOnly: true, pageSize: 8 })

  return (
    <div>
      {bannersLoading ? (
        <Skeleton className="min-h-[60vh] w-full rounded-none" />
      ) : (
        <HeroCarousel banners={banners ?? []} />
      )}

      {(categoriesLoading || (categories && categories.length > 0)) && (
        <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6">
          <h2 className="mb-6 font-serif text-2xl">{t('home.shopByCategory')}</h2>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            {categoriesLoading &&
              Array.from({ length: 4 }, (_, i) => <Skeleton key={i} className="aspect-square rounded-lg" />)}
            {categories?.map((cat) => (
              <Link
                key={cat.id}
                to={`/category/${cat.slug}`}
                className="group relative aspect-square overflow-hidden rounded-lg bg-muted"
              >
                <SafeImage
                  src={cat.imageUrl}
                  alt={cat.name}
                  className="transition-transform duration-500 group-hover:scale-105"
                />
                <div className="absolute inset-0 flex items-end bg-black/20 p-3">
                  <span className="font-medium text-white">{cat.name}</span>
                </div>
              </Link>
            ))}
          </div>
        </section>
      )}

      {(featuredLoading || (featured && featured.items.length > 0)) && (
        <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="font-serif text-2xl">{t('home.featured')}</h2>
            <Link
              to="/products?featuredOnly=true"
              className="text-sm text-muted-foreground transition-colors hover:text-foreground"
            >
              {t('common.viewAll')}
            </Link>
          </div>
          {featuredLoading ? (
            <ProductGridSkeleton />
          ) : (
            <div className="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 lg:grid-cols-4">
              {featured?.items.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}
        </section>
      )}

      {(bestSellersLoading || (bestSellers && bestSellers.items.length > 0)) && (
        <section className="mx-auto max-w-7xl px-4 py-14 sm:px-6">
          <div className="mb-6 flex items-center justify-between">
            <h2 className="font-serif text-2xl">{t('home.bestSellers')}</h2>
            <Link
              to="/products?bestSellerOnly=true"
              className="text-sm text-muted-foreground transition-colors hover:text-foreground"
            >
              {t('common.viewAll')}
            </Link>
          </div>
          {bestSellersLoading ? (
            <ProductGridSkeleton />
          ) : (
            <div className="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 lg:grid-cols-4">
              {bestSellers?.items.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}
        </section>
      )}
    </div>
  )
}
