import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'

import { pagesApi } from '@/api/pages'

export function usePage(key: string | undefined) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['pages', i18n.language, key],
    queryFn: () => pagesApi.byKey(key as string),
    enabled: !!key,
  })
}
