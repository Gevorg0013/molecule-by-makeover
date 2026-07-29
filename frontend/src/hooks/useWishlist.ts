import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { useTranslation } from 'react-i18next'

import { wishlistApi } from '@/api/wishlist'
import { useAuthStore } from '@/store/authStore'
import type { ApiError } from '@/types/dto'

const WISHLIST_KEY = ['wishlist']

export function useWishlist() {
  const isAuthed = !!useAuthStore((s) => s.accessToken)
  return useQuery({ queryKey: WISHLIST_KEY, queryFn: wishlistApi.get, enabled: isAuthed })
}

export function useToggleWishlist() {
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  return useMutation({
    mutationFn: (args: { productId: string; isWishlisted: boolean }) =>
      args.isWishlisted ? wishlistApi.remove(args.productId) : wishlistApi.add(args.productId),
    onSuccess: (_data, args) => {
      void queryClient.invalidateQueries({ queryKey: WISHLIST_KEY })
      toast.success(args.isWishlisted ? t('product.removedFromWishlist') : t('product.addedToWishlist'))
    },
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
