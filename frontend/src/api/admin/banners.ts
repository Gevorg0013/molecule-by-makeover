import { apiClient } from '../client'
import type { BannerAdminDto, BannerUpsertRequest, GalleryImageDto } from '@/types/dto'

export const adminBannersApi = {
  list: () => apiClient.get<BannerAdminDto[]>('/admin/banners').then((r) => r.data),
  create: (body: BannerUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/banners', body).then((r) => r.data),
  update: (id: string, body: BannerUpsertRequest) =>
    apiClient.put<void>(`/admin/banners/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/banners/${id}`).then((r) => r.data),
  uploadImage: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<GalleryImageDto>('/admin/banners/image', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
}
