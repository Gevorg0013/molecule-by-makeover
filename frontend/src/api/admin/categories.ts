import { apiClient } from '../client'
import type { CategoryAdminDto, CategoryUpsertRequest } from '@/types/dto'

export const adminCategoriesApi = {
  list: () => apiClient.get<CategoryAdminDto[]>('/admin/categories').then((r) => r.data),
  byId: (id: string) => apiClient.get<CategoryAdminDto>(`/admin/categories/${id}`).then((r) => r.data),
  create: (body: CategoryUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/categories', body).then((r) => r.data),
  update: (id: string, body: CategoryUpsertRequest) =>
    apiClient.put<void>(`/admin/categories/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/categories/${id}`).then((r) => r.data),
}
