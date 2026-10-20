import { apiClient } from '../client'
import type { BlogPostAdminDto, BlogPostUpsertRequest, GalleryImageDto } from '@/types/dto'

export const adminBlogApi = {
  list: () => apiClient.get<BlogPostAdminDto[]>('/admin/blog').then((r) => r.data),
  byId: (id: string) => apiClient.get<BlogPostAdminDto>(`/admin/blog/${id}`).then((r) => r.data),
  create: (body: BlogPostUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/blog', body).then((r) => r.data),
  update: (id: string, body: BlogPostUpsertRequest) =>
    apiClient.put<void>(`/admin/blog/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/blog/${id}`).then((r) => r.data),
  uploadImage: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<GalleryImageDto>('/admin/blog/image', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
}
