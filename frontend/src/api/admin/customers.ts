import { apiClient } from '../client'
import type { CustomerDetailDto, CustomerSummaryDto, PaginatedResult } from '@/types/dto'

export const adminCustomersApi = {
  list: (page = 1, pageSize = 20) =>
    apiClient
      .get<PaginatedResult<CustomerSummaryDto>>('/admin/customers', { params: { page, pageSize } })
      .then((r) => r.data),
  byId: (id: string) => apiClient.get<CustomerDetailDto>(`/admin/customers/${id}`).then((r) => r.data),
}
