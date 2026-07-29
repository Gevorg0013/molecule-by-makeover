import { apiClient } from '../client'
import type { BlogPostAdminDto, BlogPostUpsertRequest } from '@/types/dto'

export const adminBlogApi = {
  list: () => apiClient.get<BlogPostAdminDto[]>('/admin/blog').then((r) => r.data),
  byId: (id: string) => apiClient.get<BlogPostAdminDto>(`/admin/blog/${id}`).then((r) => r.data),
  create: (body: BlogPostUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/blog', body).then((r) => r.data),
  update: (id: string, body: BlogPostUpsertRequest) =>
    apiClient.put<void>(`/admin/blog/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/blog/${id}`).then((r) => r.data),
}
