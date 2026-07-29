import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminMediaApi } from '@/api/admin/media'
import type { ApiError } from '@/types/dto'

const KEY = ['admin', 'media']

export function useAdminMedia() {
  return useQuery({ queryKey: KEY, queryFn: adminMediaApi.list })
}

export function useUploadMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { file: File; altText?: string }) => adminMediaApi.upload(args.file, args.altText),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminMediaApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
