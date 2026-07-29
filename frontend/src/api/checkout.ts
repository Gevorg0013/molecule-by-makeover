import { apiClient } from './client'
import type { CheckoutRequest, CheckoutResultDto } from '@/types/dto'

export const checkoutApi = {
  checkout: (body: CheckoutRequest) => apiClient.post<CheckoutResultDto>('/checkout', body).then((r) => r.data),
}
