import { apiClient } from '../client'
import type { GalleryImageDto } from '@/types/dto'

export const adminMediaApi = {
  list: () => apiClient.get<GalleryImageDto[]>('/admin/media').then((r) => r.data),
  upload: (file: File, altText?: string) => {
    const form = new FormData()
    form.append('file', file)
    if (altText) form.append('altText', altText)
    return apiClient
      .post<GalleryImageDto>('/admin/media', form, { headers: { 'Content-Type': 'multipart/form-data' } })
      .then((r) => r.data)
  },
  remove: (id: string) => apiClient.delete<void>(`/admin/media/${id}`).then((r) => r.data),
}
