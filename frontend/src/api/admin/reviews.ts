import { apiClient } from '../client'
import type { ReviewDto } from '@/types/dto'

export const adminReviewsApi = {
  list: (approvedOnly?: boolean) =>
    apiClient.get<ReviewDto[]>('/admin/reviews', { params: { approvedOnly } }).then((r) => r.data),
  approve: (id: string) => apiClient.put<void>(`/admin/reviews/${id}/approve`).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/reviews/${id}`).then((r) => r.data),
}
