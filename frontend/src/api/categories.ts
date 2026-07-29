import { apiClient } from './client'
import type { CategoryDto } from '@/types/dto'

export const categoriesApi = {
  list: () => apiClient.get<CategoryDto[]>('/categories').then((r) => r.data),
  bySlug: (slug: string) => apiClient.get<CategoryDto>(`/categories/${slug}`).then((r) => r.data),
}
