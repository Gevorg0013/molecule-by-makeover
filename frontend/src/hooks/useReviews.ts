import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { reviewsApi } from '@/api/reviews'
import type { CreateReviewRequest } from '@/types/dto'

export function useReviews(productId: string | undefined) {
  return useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewsApi.list(productId as string),
    enabled: !!productId,
  })
}

export function useCreateReview(productId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateReviewRequest) => reviewsApi.create(productId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['reviews', productId] }),
  })
}
