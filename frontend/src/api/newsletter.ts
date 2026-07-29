import { apiClient } from './client'

export const newsletterApi = {
  subscribe: (email: string) => apiClient.post<void>('/newsletter/subscribe', { email }).then((r) => r.data),
}
