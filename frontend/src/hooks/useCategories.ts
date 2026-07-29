import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'

import { categoriesApi } from '@/api/categories'

export function useCategories() {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['categories', i18n.language],
    queryFn: categoriesApi.list,
  })
}

export function useCategory(slug: string | undefined) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['categories', i18n.language, slug],
    queryFn: () => categoriesApi.bySlug(slug as string),
    enabled: !!slug,
  })
}
