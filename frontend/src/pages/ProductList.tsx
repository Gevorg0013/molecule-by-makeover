import { SearchX } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useParams, useSearchParams } from 'react-router-dom'

import { ProductCard } from '@/components/storefront/ProductCard'
import { ProductGridSkeleton } from '@/components/storefront/ProductCardSkeleton'
import { EmptyState } from '@/components/common/EmptyState'
import { ErrorState } from '@/components/common/ErrorState'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { cn } from '@/lib/utils'
import { useCategories } from '@/hooks/useCategories'
import { useProductList } from '@/hooks/useProducts'
import { ProductSort, productSortLabels } from '@/types/enums'

export function ProductListPage() {
  const { t } = useTranslation()
  const { categorySlug: routeCategorySlug } = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const { data: categories } = useCategories()

  const page = Number(searchParams.get('page') ?? 1)
  const categorySlug = routeCategorySlug ?? searchParams.get('categorySlug') ?? undefined
  const search = searchParams.get('search') ?? undefined
  const minPrice = searchParams.get('minPrice') ?? ''
  const maxPrice = searchParams.get('maxPrice') ?? ''
  const sort = Number(searchParams.get('sort') ?? ProductSort.Newest)
  const featuredOnly = searchParams.get('featuredOnly') === 'true'
  const bestSellerOnly = searchParams.get('bestSellerOnly') === 'true'

  const { data, isLoading, isError, refetch } = useProductList({
    page,
    pageSize: 12,
    categorySlug,
    search,
    minPrice: minPrice ? Number(minPrice) : undefined,
    maxPrice: maxPrice ? Number(maxPrice) : undefined,
    sort,
    featuredOnly: featuredOnly || undefined,
    bestSellerOnly: bestSellerOnly || undefined,
  })

  function updateParam(key: string, value: string | null) {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    next.delete('page')
    setSearchParams(next)
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6">
      <div className="flex flex-col gap-8 md:flex-row">
        <aside className="w-full shrink-0 md:w-56">
          <h2 className="mb-3 font-medium">{t('product.filters')}</h2>
          <div className="flex flex-col gap-4">
            <div>
              <p className="mb-1.5 text-sm text-muted-foreground">{t('product.category')}</p>
              <div className="flex flex-col gap-0.5">
                <button
                  onClick={() => updateParam('categorySlug', null)}
                  className={cn(
                    'rounded-md px-2 py-1.5 text-left text-sm transition-colors',
                    !categorySlug ? 'bg-accent/15 font-medium text-foreground' : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
                  )}
                >
                  {t('common.all')}
                </button>
                {categories?.map((c) => (
                  <button
                    key={c.id}
                    onClick={() => updateParam('categorySlug', c.slug)}
                    className={cn(
                      'rounded-md px-2 py-1.5 text-left text-sm transition-colors',
                      categorySlug === c.slug
                        ? 'bg-accent/15 font-medium text-foreground'
                        : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
                    )}
                  >
                    {c.name}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <p className="mb-1.5 text-sm text-muted-foreground">{t('product.priceRange')}</p>
              <div className="flex items-center gap-2">
                <Input
                  type="number"
                  placeholder="Min"
                  defaultValue={minPrice}
                  onBlur={(e) => updateParam('minPrice', e.target.value || null)}
                  className="w-full"
                />
                <Input
                  type="number"
                  placeholder="Max"
                  defaultValue={maxPrice}
                  onBlur={(e) => updateParam('maxPrice', e.target.value || null)}
                  className="w-full"
                />
              </div>
            </div>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSearchParams(new URLSearchParams())}
              className="self-start px-0"
            >
              {t('product.clearFilters')}
            </Button>
          </div>
        </aside>

        <div className="flex-1">
          <div className="mb-6 flex items-center justify-between">
            <p className="text-sm text-muted-foreground">{data ? data.totalCount : ''}</p>
            <Select value={String(sort)} onValueChange={(v) => updateParam('sort', v)}>
              <SelectTrigger size="sm" className="w-48">
                <SelectValue placeholder={t('product.sortBy')} />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(productSortLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {isLoading && <ProductGridSkeleton />}

          {isError && <ErrorState onRetry={() => refetch()} />}

          {!isLoading && !isError && data && data.items.length === 0 && (
            <EmptyState
              icon={SearchX}
              title={t('common.noResults')}
              action={
                <Button variant="outline" size="sm" onClick={() => setSearchParams(new URLSearchParams())}>
                  {t('product.clearFilters')}
                </Button>
              }
            />
          )}

          {!isLoading && !isError && data && data.items.length > 0 && (
            <div className="grid grid-cols-2 gap-x-4 gap-y-8 sm:grid-cols-3 lg:grid-cols-4">
              {data.items.map((p) => (
                <ProductCard key={p.id} product={p} />
              ))}
            </div>
          )}

          {data && data.totalPages > 1 && (
            <div className="mt-10 flex items-center justify-center gap-3">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => updateParam('page', String(page - 1))}
              >
                {t('common.previous')}
              </Button>
              <span className="text-sm text-muted-foreground">
                {t('common.page')} {page} {t('common.of')} {data.totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= data.totalPages}
                onClick={() => updateParam('page', String(page + 1))}
              >
                {t('common.next')}
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
