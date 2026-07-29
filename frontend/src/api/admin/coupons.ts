import { apiClient } from '../client'
import type { CouponDto, CouponUpsertRequest } from '@/types/dto'

export const adminCouponsApi = {
  list: () => apiClient.get<CouponDto[]>('/admin/coupons').then((r) => r.data),
  byId: (id: string) => apiClient.get<CouponDto>(`/admin/coupons/${id}`).then((r) => r.data),
  create: (body: CouponUpsertRequest) =>
    apiClient.post<{ id: string }>('/admin/coupons', body).then((r) => r.data),
  update: (id: string, body: CouponUpsertRequest) =>
    apiClient.put<void>(`/admin/coupons/${id}`, body).then((r) => r.data),
  remove: (id: string) => apiClient.delete<void>(`/admin/coupons/${id}`).then((r) => r.data),
}
