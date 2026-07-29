import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminCategoriesApi } from '@/api/admin/categories'
import type { ApiError, CategoryUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'categories']

export function useAdminCategories() {
  return useQuery({ queryKey: KEY, queryFn: adminCategoriesApi.list })
}

export function useAdminCategory(id: string | undefined) {
  return useQuery({
    queryKey: [...KEY, id],
    queryFn: () => adminCategoriesApi.byId(id as string),
    enabled: !!id,
  })
}

export function useCreateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CategoryUpsertRequest) => adminCategoriesApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdateCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: CategoryUpsertRequest }) =>
      adminCategoriesApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminCategoriesApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
