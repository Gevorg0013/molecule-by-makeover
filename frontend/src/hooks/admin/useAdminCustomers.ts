import { useQuery } from '@tanstack/react-query'

import { adminCustomersApi } from '@/api/admin/customers'

export function useAdminCustomers(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['admin', 'customers', page, pageSize],
    queryFn: () => adminCustomersApi.list(page, pageSize),
  })
}

export function useAdminCustomer(id: string | undefined) {
  return useQuery({
    queryKey: ['admin', 'customers', id],
    queryFn: () => adminCustomersApi.byId(id as string),
    enabled: !!id,
  })
}
