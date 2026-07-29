import { apiClient } from '../client'
import type { BannerAdminDto, BannerUpsertRequest } from '@/types/dto'

export const adminBannersApi = {
  list: () => apiClient.get<BannerAdminDto[]>('/admin/banners').then((r) => r.data),
  create: (body: BannerUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/banners', body).then((r) => r.data),
  update: (id: string, body: BannerUpsertRequest) =>
    apiClient.put<void>(`/admin/banners/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/banners/${id}`).then((r) => r.data),
}
