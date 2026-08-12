import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios'

import i18n from '@/i18n'
import { useAuthStore } from '@/store/authStore'
import type { ApiError, AuthResponse } from '@/types/dto'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5116/api/v1'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
})

apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  config.params = { ...(config.params as Record<string, unknown> | undefined), lang: i18n.language || 'en' }
  return config
})

type RetryableConfig = InternalAxiosRequestConfig & { _retry?: boolean }

let refreshPromise: Promise<string | null> | null = null

async function performRefresh(): Promise<string | null> {
  try {
    const response = await axios.post<AuthResponse>(`${API_BASE_URL}/auth/refresh`, undefined, {
      withCredentials: true,
    })
    useAuthStore.getState().setSession(response.data.accessToken, response.data.user)
    return response.data.accessToken
  } catch {
    useAuthStore.getState().clearSession()
    return null
  }
}

// Shared by the 401 interceptor below and useBootstrapAuth so concurrent
// callers (e.g. React StrictMode's double effect invocation) coalesce into a
// single /auth/refresh call instead of racing and tripping the backend's
// refresh-token reuse detection, which revokes all sessions.
export function refreshSession(): Promise<string | null> {
  refreshPromise ??= performRefresh().finally(() => {
    refreshPromise = null
  })
  return refreshPromise
}

const NO_REFRESH_PATHS = ['/auth/refresh', '/auth/login', '/auth/register']

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetryableConfig | undefined
    const isExempt = NO_REFRESH_PATHS.some((path) => original?.url?.includes(path))

    if (error.response?.status === 401 && original && !original._retry && !isExempt) {
      original._retry = true
      const token = await refreshSession()
      if (token) {
        original.headers.set('Authorization', `Bearer ${token}`)
        return apiClient(original)
      }
    }

    return Promise.reject(normalizeError(error))
  },
)

interface ProblemDetailsBody {
  title?: string
  detail?: string | null
  errors?: Record<string, string[]>
}

function normalizeError(error: AxiosError): ApiError {
  const data = error.response?.data as ProblemDetailsBody | undefined
  return {
    status: error.response?.status ?? 0,
    title: data?.title ?? error.message ?? 'Request failed',
    detail: data?.detail ?? null,
    errors: data?.errors,
  }
}
