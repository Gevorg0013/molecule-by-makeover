import { apiClient } from './client'
import type { AuthResponse, LoginRequest, MeResponse, RegisterRequest } from '@/types/dto'

export const authApi = {
  register: (body: RegisterRequest) => apiClient.post<AuthResponse>('/auth/register', body).then((r) => r.data),
  login: (body: LoginRequest) => apiClient.post<AuthResponse>('/auth/login', body).then((r) => r.data),
  refresh: () => apiClient.post<AuthResponse>('/auth/refresh').then((r) => r.data),
  logout: () => apiClient.post<void>('/auth/logout').then((r) => r.data),
  me: () => apiClient.get<MeResponse>('/auth/me').then((r) => r.data),
}
