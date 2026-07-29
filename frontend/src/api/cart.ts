import { apiClient } from './client'
import type { CartDto } from '@/types/dto'

export const cartApi = {
  get: () => apiClient.get<CartDto>('/cart').then((r) => r.data),
  addItem: (productId: string, quantity: number) =>
    apiClient.post<CartDto>('/cart/items', { productId, quantity }).then((r) => r.data),
  updateItem: (itemId: string, quantity: number) =>
    apiClient.patch<CartDto>(`/cart/items/${itemId}`, { quantity }).then((r) => r.data),
  removeItem: (itemId: string) => apiClient.delete<CartDto>(`/cart/items/${itemId}`).then((r) => r.data),
  applyCoupon: (code: string) => apiClient.post<CartDto>('/cart/coupon', { code }).then((r) => r.data),
  removeCoupon: () => apiClient.delete<CartDto>('/cart/coupon').then((r) => r.data),
}
