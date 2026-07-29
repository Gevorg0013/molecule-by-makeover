import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminOrdersApi } from '@/api/admin/orders'
import type { ApiError } from '@/types/dto'
import type { OrderStatus } from '@/types/enums'

export function useAdminOrders(page = 1, pageSize = 20, status?: OrderStatus) {
  return useQuery({
    queryKey: ['admin', 'orders', page, pageSize, status],
    queryFn: () => adminOrdersApi.list(page, pageSize, status),
  })
}

export function useAdminOrder(orderNumber: string | undefined) {
  return useQuery({
    queryKey: ['admin', 'orders', orderNumber],
    queryFn: () => adminOrdersApi.byNumber(orderNumber as string),
    enabled: !!orderNumber,
  })
}

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (args: { id: string; status: OrderStatus }) =>
      adminOrdersApi.updateStatus(args.id, { status: args.status }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
