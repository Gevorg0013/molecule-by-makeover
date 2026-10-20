import { apiClient } from '../client'
import type { CategoryAdminDto, CategoryUpsertRequest, GalleryImageDto } from '@/types/dto'

export const adminCategoriesApi = {
  list: () => apiClient.get<CategoryAdminDto[]>('/admin/categories').then((r) => r.data),
  byId: (id: string) => apiClient.get<CategoryAdminDto>(`/admin/categories/${id}`).then((r) => r.data),
  create: (body: CategoryUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/categories', body).then((r) => r.data),
  update: (id: string, body: CategoryUpsertRequest) =>
    apiClient.put<void>(`/admin/categories/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/categories/${id}`).then((r) => r.data),
  uploadImage: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<GalleryImageDto>('/admin/categories/image', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
}
