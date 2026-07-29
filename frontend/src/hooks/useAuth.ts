import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'

import { authApi } from '@/api/auth'
import { useAuthStore } from '@/store/authStore'
import type { ApiError, LoginRequest, RegisterRequest } from '@/types/dto'

export function useBootstrapAuth() {
  const isBootstrapped = useAuthStore((s) => s.isBootstrapped)
  const setBootstrapped = useAuthStore((s) => s.setBootstrapped)
  const setSession = useAuthStore((s) => s.setSession)

  useEffect(() => {
    if (isBootstrapped) return
    authApi
      .refresh()
      .then((res) => setSession(res.accessToken, res.user))
      .catch(() => {
        // no active session — that's fine, user stays logged out
      })
      .finally(() => setBootstrapped())
  }, [isBootstrapped, setBootstrapped, setSession])

  return isBootstrapped
}

export function useLogin() {
  const setSession = useAuthStore((s) => s.setSession)
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: LoginRequest) => authApi.login(body),
    onSuccess: (res) => {
      setSession(res.accessToken, res.user)
      void queryClient.invalidateQueries({ queryKey: ['cart'] })
      void queryClient.invalidateQueries({ queryKey: ['wishlist'] })
    },
  })
}

export function useRegister() {
  const setSession = useAuthStore((s) => s.setSession)
  return useMutation({
    mutationFn: (body: RegisterRequest) => authApi.register(body),
    onSuccess: (res) => setSession(res.accessToken, res.user),
  })
}

export function useLogout() {
  const clearSession = useAuthStore((s) => s.clearSession)
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => authApi.logout(),
    onSettled: () => {
      clearSession()
      queryClient.clear()
    },
  })
}

export function authErrorMessage(error: unknown): string {
  const apiError = error as ApiError
  return apiError?.detail ?? apiError?.title ?? 'Something went wrong'
}
