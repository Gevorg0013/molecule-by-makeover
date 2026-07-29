import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'

import { blogApi } from '@/api/blog'

export function useBlogList(page = 1, pageSize = 10) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['blog', i18n.language, page, pageSize],
    queryFn: () => blogApi.list(page, pageSize),
  })
}

export function useBlogPost(slug: string | undefined) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['blog', i18n.language, slug],
    queryFn: () => blogApi.bySlug(slug as string),
    enabled: !!slug,
  })
}
