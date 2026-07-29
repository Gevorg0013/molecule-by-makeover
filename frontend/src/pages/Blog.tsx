import { Newspaper } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router-dom'

import { EmptyState } from '@/components/common/EmptyState'
import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { useBlogList } from '@/hooks/useBlog'
import { formatDate } from '@/lib/date'

export function BlogPage() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const page = Number(searchParams.get('page') ?? 1)
  const { data, isLoading } = useBlogList(page, 9)

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
      <h1 className="mb-8 font-serif text-3xl">{t('blog.title')}</h1>

      {isLoading && (
        <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }, (_, i) => (
            <div key={i} className="flex flex-col gap-3">
              <Skeleton className="aspect-[4/3] w-full rounded-lg" />
              <Skeleton className="h-3 w-20" />
              <Skeleton className="h-5 w-3/4" />
            </div>
          ))}
        </div>
      )}

      {!isLoading && data && data.items.length === 0 && <EmptyState icon={Newspaper} title={t('common.noResults')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((post) => (
            <Link key={post.id} to={`/blog/${post.slug}`} className="group flex flex-col gap-3">
              <div className="aspect-[4/3] overflow-hidden rounded-lg bg-muted">
                <SafeImage
                  src={post.coverImageUrl}
                  alt={post.title}
                  fallbackLabel={post.title}
                  className="transition-transform duration-500 group-hover:scale-105"
                />
              </div>
              <div>
                <p className="text-xs text-muted-foreground">{formatDate(post.publishedAt)}</p>
                <h2 className="font-serif text-lg transition-colors group-hover:text-accent">{post.title}</h2>
                {post.excerpt && <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">{post.excerpt}</p>}
              </div>
            </Link>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-10 flex items-center justify-center gap-3">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => setSearchParams({ page: String(page - 1) })}
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
            onClick={() => setSearchParams({ page: String(page + 1) })}
          >
            {t('common.next')}
          </Button>
        </div>
      )}
    </div>
  )
}
