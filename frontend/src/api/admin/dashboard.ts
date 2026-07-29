import { apiClient } from '../client'
import type { DashboardStatsDto } from '@/types/dto'

export const adminDashboardApi = {
  stats: () => apiClient.get<DashboardStatsDto>('/admin/dashboard/stats').then((r) => r.data),
}
