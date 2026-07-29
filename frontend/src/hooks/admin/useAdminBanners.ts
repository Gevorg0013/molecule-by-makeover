import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminBannersApi } from '@/api/admin/banners'
import type { ApiError, BannerUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'banners']

export function useAdminBanners() {
  return useQuery({ queryKey: KEY, queryFn: adminBannersApi.list })
}

export function useCreateBanner() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: BannerUpsertRequest) => adminBannersApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdateBanner() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: BannerUpsertRequest }) => adminBannersApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteBanner() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminBannersApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
