import { apiClient } from './client'
import type { BlogPostDetailDto, BlogPostListItemDto, PaginatedResult } from '@/types/dto'

export const blogApi = {
  list: (page = 1, pageSize = 10) =>
    apiClient.get<PaginatedResult<BlogPostListItemDto>>('/blog', { params: { page, pageSize } }).then((r) => r.data),
  bySlug: (slug: string) => apiClient.get<BlogPostDetailDto>(`/blog/${slug}`).then((r) => r.data),
}
