import { apiClient } from './client'
import type { WishlistDto } from '@/types/dto'

export const wishlistApi = {
  get: () => apiClient.get<WishlistDto>('/wishlist').then((r) => r.data),
  add: (productId: string) => apiClient.post<void>(`/wishlist/${productId}`).then((r) => r.data),
  remove: (productId: string) => apiClient.delete<void>(`/wishlist/${productId}`).then((r) => r.data),
}
