import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'

import { authApi } from '@/api/auth'
import { refreshSession } from '@/api/client'
import { useAuthStore } from '@/store/authStore'
import type { ApiError, ForgotPasswordRequest, LoginRequest, RegisterRequest, ResetPasswordRequest } from '@/types/dto'

export function useBootstrapAuth() {
  const isBootstrapped = useAuthStore((s) => s.isBootstrapped)
  const setBootstrapped = useAuthStore((s) => s.setBootstrapped)

  useEffect(() => {
    if (isBootstrapped) return
    // Goes through the shared refreshSession() lock (not authApi.refresh()
    // directly) so this doesn't race the 401 interceptor's own refresh call,
    // or itself under React StrictMode's double effect invocation — a race
    // that trips the backend's refresh-token reuse detection and revokes
    // every session.
    refreshSession().finally(() => setBootstrapped())
  }, [isBootstrapped, setBootstrapped])

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

export function useForgotPassword() {
  return useMutation({
    mutationFn: (body: ForgotPasswordRequest) => authApi.forgotPassword(body),
  })
}

export function useResetPassword() {
  return useMutation({
    mutationFn: (body: ResetPasswordRequest) => authApi.resetPassword(body),
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
