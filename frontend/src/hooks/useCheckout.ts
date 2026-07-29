import { useMutation } from '@tanstack/react-query'

import { checkoutApi } from '@/api/checkout'
import type { CheckoutRequest } from '@/types/dto'

export function useCheckout() {
  return useMutation({ mutationFn: (body: CheckoutRequest) => checkoutApi.checkout(body) })
}
