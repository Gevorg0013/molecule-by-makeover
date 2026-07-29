import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminPagesApi } from '@/api/admin/pages'
import type { ApiError, PageUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'pages']

export function useAdminPagesList() {
  return useQuery({ queryKey: KEY, queryFn: adminPagesApi.list })
}

export function useAdminPage(id: string | undefined) {
  return useQuery({
    queryKey: [...KEY, id],
    queryFn: () => adminPagesApi.byId(id as string),
    enabled: !!id,
  })
}

export function useCreatePage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: PageUpsertRequest) => adminPagesApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdatePage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: PageUpsertRequest }) => adminPagesApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeletePage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminPagesApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
