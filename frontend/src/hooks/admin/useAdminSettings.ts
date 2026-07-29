import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminLanguagesApi, adminSettingsApi } from '@/api/admin/settings'
import type { ApiError, LanguageUpsertRequest, UpdateSettingRequest } from '@/types/dto'

export function useAdminSettings(group: string) {
  return useQuery({ queryKey: ['admin', 'settings', group], queryFn: () => adminSettingsApi.byGroup(group) })
}

export function useUpdateSetting(group: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { key: string; body: UpdateSettingRequest }) =>
      adminSettingsApi.update(group, args.key, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'settings', group] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useAdminLanguages() {
  return useQuery({ queryKey: ['admin', 'languages'], queryFn: adminLanguagesApi.list })
}

export function useCreateLanguage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: LanguageUpsertRequest) => adminLanguagesApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'languages'] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdateLanguage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: number; body: LanguageUpsertRequest }) =>
      adminLanguagesApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'languages'] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
