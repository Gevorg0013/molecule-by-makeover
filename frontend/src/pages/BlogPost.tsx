import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { SafeImage } from '@/components/common/SafeImage'
import { useBlogPost } from '@/hooks/useBlog'
import { formatDate } from '@/lib/date'

export function BlogPostPage() {
  const { t } = useTranslation()
  const { slug } = useParams()
  const { data: post, isLoading } = useBlogPost(slug)

  if (isLoading) return <div className="px-4 py-20 text-center text-muted-foreground">{t('common.loading')}</div>
  if (!post) return null

  return (
    <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      {post.coverImageUrl && (
        <div className="mb-8 aspect-[16/9] overflow-hidden rounded-lg bg-muted">
          <SafeImage src={post.coverImageUrl} alt={post.title} priority />
        </div>
      )}
      <p className="text-sm text-muted-foreground">{formatDate(post.publishedAt)}</p>
      <h1 className="mt-1 font-serif text-3xl">{post.title}</h1>
      <div
        className="mt-8 max-w-none leading-relaxed text-foreground/90 [&_h2]:mt-8 [&_h2]:mb-3 [&_h2]:font-serif [&_h2]:text-xl [&_p]:mb-4"
        dangerouslySetInnerHTML={{ __html: post.content }}
      />
    </article>
  )
}
