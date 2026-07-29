import { apiClient } from './client'
import type { BannerDto } from '@/types/dto'

export const bannersApi = {
  active: () => apiClient.get<BannerDto[]>('/banners/active').then((r) => r.data),
}
