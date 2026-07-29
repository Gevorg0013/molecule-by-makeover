import { apiClient } from '../client'
import type { LanguageDto, LanguageUpsertRequest, SettingDto, UpdateSettingRequest } from '@/types/dto'

export const adminSettingsApi = {
  byGroup: (group: string) => apiClient.get<SettingDto[]>(`/admin/settings/${group}`).then((r) => r.data),
  update: (group: string, key: string, body: UpdateSettingRequest) =>
    apiClient.put<void>(`/admin/settings/${group}/${key}`, body).then((r) => r.data),
}

export const adminLanguagesApi = {
  list: () => apiClient.get<LanguageDto[]>('/admin/languages').then((r) => r.data),
  create: (body: LanguageUpsertRequest) =>
    apiClient.post<{ id: number }>('/admin/languages', body).then((r) => r.data),
  update: (id: number, body: LanguageUpsertRequest) =>
    apiClient.put<void>(`/admin/languages/${id}`, body).then((r) => r.data),
}
