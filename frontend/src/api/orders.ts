import { apiClient } from './client'
import type { OrderDetailDto, OrderSummaryDto, PaginatedResult } from '@/types/dto'

export const ordersApi = {
  list: (page = 1, pageSize = 20) =>
    apiClient.get<PaginatedResult<OrderSummaryDto>>('/orders', { params: { page, pageSize } }).then((r) => r.data),
  byNumber: (orderNumber: string) => apiClient.get<OrderDetailDto>(`/orders/${orderNumber}`).then((r) => r.data),
}
