import { apiClient } from './client'
import type { PageDto } from '@/types/dto'

export const pagesApi = {
  byKey: (key: string) => apiClient.get<PageDto>(`/pages/${key}`).then((r) => r.data),
}
