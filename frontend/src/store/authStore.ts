import { create } from 'zustand'

import type { UserProfileDto } from '@/types/dto'

interface AuthState {
  accessToken: string | null
  user: UserProfileDto | null
  isBootstrapped: boolean
  setSession: (accessToken: string, user: UserProfileDto) => void
  clearSession: () => void
  setBootstrapped: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  user: null,
  isBootstrapped: false,
  setSession: (accessToken, user) => set({ accessToken, user }),
  clearSession: () => set({ accessToken: null, user: null }),
  setBootstrapped: () => set({ isBootstrapped: true }),
}))

export function isAdmin(user: UserProfileDto | null): boolean {
  return user?.roles.includes('Admin') ?? false
}
