import { apiClient } from '../client'
import type { PageAdminDto, PageUpsertRequest } from '@/types/dto'

export const adminPagesApi = {
  list: () => apiClient.get<PageAdminDto[]>('/admin/pages').then((r) => r.data),
  byId: (id: string) => apiClient.get<PageAdminDto>(`/admin/pages/${id}`).then((r) => r.data),
  create: (body: PageUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/pages', body).then((r) => r.data),
  update: (id: string, body: PageUpsertRequest) =>
    apiClient.put<void>(`/admin/pages/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/pages/${id}`).then((r) => r.data),
}
