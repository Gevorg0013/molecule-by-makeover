import { useQuery } from '@tanstack/react-query'

import { ordersApi } from '@/api/orders'

export function useOrders(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['orders', page, pageSize],
    queryFn: () => ordersApi.list(page, pageSize),
  })
}

export function useOrder(orderNumber: string | undefined) {
  return useQuery({
    queryKey: ['orders', orderNumber],
    queryFn: () => ordersApi.byNumber(orderNumber as string),
    enabled: !!orderNumber,
  })
}
