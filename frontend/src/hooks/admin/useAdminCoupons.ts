import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminCouponsApi } from '@/api/admin/coupons'
import type { ApiError, CouponUpsertRequest } from '@/types/dto'

const KEY = ['admin', 'coupons']

export function useAdminCoupons() {
  return useQuery({ queryKey: KEY, queryFn: adminCouponsApi.list })
}

export function useAdminCoupon(id: string | undefined) {
  return useQuery({
    queryKey: [...KEY, id],
    queryFn: () => adminCouponsApi.byId(id as string),
    enabled: !!id,
  })
}

export function useCreateCoupon() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CouponUpsertRequest) => adminCouponsApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useUpdateCoupon() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; body: CouponUpsertRequest }) => adminCouponsApi.update(args.id, args.body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteCoupon() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminCouponsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
