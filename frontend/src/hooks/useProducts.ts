import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'

import { productsApi } from '@/api/products'
import type { ProductQueryParams } from '@/types/dto'

export function useProductList(params: ProductQueryParams) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['products', i18n.language, params],
    queryFn: () => productsApi.list(params),
    placeholderData: (prev) => prev,
  })
}

export function useProduct(slug: string | undefined) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['products', i18n.language, 'slug', slug],
    queryFn: () => productsApi.bySlug(slug as string),
    enabled: !!slug,
  })
}

export function useRelatedProducts(id: string | undefined) {
  const { i18n } = useTranslation()
  return useQuery({
    queryKey: ['products', i18n.language, 'related', id],
    queryFn: () => productsApi.related(id as string),
    enabled: !!id,
  })
}
