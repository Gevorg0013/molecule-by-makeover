import { apiClient } from './client'
import type { CreateReviewRequest, ReviewDto } from '@/types/dto'

export const reviewsApi = {
  list: (productId: string) => apiClient.get<ReviewDto[]>(`/products/${productId}/reviews`).then((r) => r.data),
  create: (productId: string, body: CreateReviewRequest) =>
    apiClient.post<{ id: string }>(`/products/${productId}/reviews`, body).then((r) => r.data),
}
