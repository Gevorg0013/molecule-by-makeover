import { apiClient } from '../client'
import type { OrderDetailDto, OrderSummaryDto, PaginatedResult, UpdateOrderStatusRequest } from '@/types/dto'
import type { OrderStatus } from '@/types/enums'

export const adminOrdersApi = {
  list: (page = 1, pageSize = 20, status?: OrderStatus) =>
    apiClient
      .get<PaginatedResult<OrderSummaryDto>>('/admin/orders', { params: { page, pageSize, status } })
      .then((r) => r.data),
  byNumber: (orderNumber: string) =>
    apiClient.get<OrderDetailDto>(`/admin/orders/${orderNumber}`).then((r) => r.data),
  updateStatus: (id: string, body: UpdateOrderStatusRequest) =>
    apiClient.put<void>(`/admin/orders/${id}/status`, body).then((r) => r.data),
}
