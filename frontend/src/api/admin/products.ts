import { apiClient } from '../client'
import type { GalleryImageDto, ProductAdminDto, ProductImageDto, ProductUpsertRequest } from '@/types/dto'

export const adminProductsApi = {
  list: () => apiClient.get<ProductAdminDto[]>('/admin/products').then((r) => r.data),
  byId: (id: string) => apiClient.get<ProductAdminDto>(`/admin/products/${id}`).then((r) => r.data),
  create: (body: ProductUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/products', body).then((r) => r.data),
  update: (id: string, body: ProductUpsertRequest) =>
    apiClient.put<void>(`/admin/products/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/products/${id}`).then((r) => r.data),
  uploadMainImage: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<GalleryImageDto>('/admin/products/main-image', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
  uploadImage: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<ProductImageDto>(`/admin/products/${id}/images`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data)
  },
  removeImage: (id: string, imageId: string) =>
    apiClient.delete<void>(`/admin/products/${id}/images/${imageId}`).then((r) => r.data),
}
