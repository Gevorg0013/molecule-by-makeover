import { apiClient } from './client'
import type { PaginatedResult, ProductDetailDto, ProductListItemDto, ProductQueryParams } from '@/types/dto'

export const productsApi = {
  list: (params: ProductQueryParams) =>
    apiClient.get<PaginatedResult<ProductListItemDto>>('/products', { params }).then((r) => r.data),
  bySlug: (slug: string) => apiClient.get<ProductDetailDto>(`/products/${slug}`).then((r) => r.data),
  related: (id: string) => apiClient.get<ProductListItemDto[]>(`/products/${id}/related`).then((r) => r.data),
}
