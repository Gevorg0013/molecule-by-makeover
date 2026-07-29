import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'

import { adminReviewsApi } from '@/api/admin/reviews'
import type { ApiError } from '@/types/dto'

const KEY = ['admin', 'reviews']

export function useAdminReviews(approvedOnly?: boolean) {
  return useQuery({ queryKey: [...KEY, approvedOnly], queryFn: () => adminReviewsApi.list(approvedOnly) })
}

export function useApproveReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminReviewsApi.approve(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}

export function useDeleteReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => adminReviewsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: KEY }),
    onError: (error: ApiError) => toast.error(error.detail ?? error.title),
  })
}
