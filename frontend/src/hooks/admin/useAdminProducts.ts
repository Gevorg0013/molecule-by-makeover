import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminProductsApi } from '@/api/admin/products'
import { formatApiError } from '@/lib/apiError'
import type { ApiError, ProductUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'products']

export function useAdminProducts() {
  return useQuery({ queryKey: KEY, queryFn: adminProductsApi.list })
}

export function useAdminProduct(id: string | undefined) {
  return useQuery({
    queryKey: [...KEY, id],
    queryFn: () => adminProductsApi.byId(id as string),
    enabled: !!id,
  })
}

export function useCreateProduct() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: ProductUpsertRequest) => adminProductsApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(formatApiError(error)),
  })
}

export function useUpdateProduct() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: ProductUpsertRequest }) =>
      adminProductsApi.update(args.id, args.body),
    onSuccess: (_d, args) => {
      void queryClient.invalidateQueries({ queryKey: KEY })
      void queryClient.invalidateQueries({ queryKey: [...KEY, args.id] })
    },
    onError: (error: ApiError) => toast.error(formatApiError(error)),
  })
}

export function useDeleteProduct() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminProductsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

// The resulting URL is held in form state and persisted by the product upsert, so there is
// no product-scoped cache to invalidate here.
export function useUploadProductMainImage() {
  return useMutation({
    mutationFn: (file: File) => adminProductsApi.uploadMainImage(file),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUploadProductImage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; file: File }) => adminProductsApi.uploadImage(args.id, args.file),
    onSuccess: (_d, args) => queryClient.invalidateQueries({ queryKey: [...KEY, args.id] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteProductImage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; imageId: string }) => adminProductsApi.removeImage(args.id, args.imageId),
    onSuccess: (_d, args) => queryClient.invalidateQueries({ queryKey: [...KEY, args.id] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
