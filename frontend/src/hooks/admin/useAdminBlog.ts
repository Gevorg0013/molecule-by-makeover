import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminBlogApi } from '@/api/admin/blog'
import type { ApiError, BlogPostUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'blog']

export function useAdminBlogList() {
  return useQuery({ queryKey: KEY, queryFn: adminBlogApi.list })
}

export function useAdminBlogPost(id: string | undefined) {
  return useQuery({
    queryKey: [...KEY, id],
    queryFn: () => adminBlogApi.byId(id as string),
    enabled: !!id,
  })
}

export function useCreateBlogPost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: BlogPostUpsertRequest) => adminBlogApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdateBlogPost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: BlogPostUpsertRequest }) => adminBlogApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteBlogPost() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminBlogApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
