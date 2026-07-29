import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'

import { bannersApi } from '@/api/banners'

export function useActiveBanners() {
  const { i18n } = useTranslation()
  return useQuery({ queryKey: ['banners', 'active', i18n.language], queryFn: bannersApi.active })
}
