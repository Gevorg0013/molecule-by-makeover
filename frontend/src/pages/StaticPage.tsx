import { useParams } from 'react-router-dom'

import { LoadingState } from '@/components/common/Spinner'
import { usePage } from '@/hooks/usePages'
import { NotFoundPage } from './NotFound'

export function StaticPagePage() {
  const { key } = useParams()
  const { data: page, isLoading, isError } = usePage(key)

  if (isLoading) return <LoadingState variant="page" />
  if (isError || !page) return <NotFoundPage />

  return (
    <article className="mx-auto max-w-3xl px-4 py-14 sm:px-6">
      <h1 className="font-serif text-3xl">{page.title}</h1>
      <div
        className="mt-8 leading-relaxed text-foreground/90 [&_h2]:mt-8 [&_h2]:mb-3 [&_h2]:font-serif [&_h2]:text-xl [&_p]:mb-4"
        dangerouslySetInnerHTML={{ __html: page.content }}
      />
    </article>
  )
}
