import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { cartApi } from '@/api/cart'
import type { ApiError, CartDto } from '@/types/dto'

const CART_KEY = ['cart']

export function useCart() {
  return useQuery({ queryKey: CART_KEY, queryFn: cartApi.get })
}

function useCartMutation<TArgs>(fn: (args: TArgs) => Promise<CartDto>) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: fn,
    onSuccess: (data) => queryClient.setQueryData(CART_KEY, data),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useAddCartItem() {
  return useCartMutation((args: { productId: string; quantity: number }) =>
    cartApi.addItem(args.productId, args.quantity),
  )
}

export function useUpdateCartItem() {
  return useCartMutation((args: { itemId: string; quantity: number }) =>
    cartApi.updateItem(args.itemId, args.quantity),
  )
}

export function useRemoveCartItem() {
  return useCartMutation((itemId: string) => cartApi.removeItem(itemId))
}

export function useApplyCoupon() {
  return useCartMutation((code: string) => cartApi.applyCoupon(code))
}

export function useRemoveCoupon() {
  return useCartMutation<void>(() => cartApi.removeCoupon())
}
