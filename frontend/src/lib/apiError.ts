import type { ApiError } from '@/types/dto'

// ValidationProblemDetails leaves `detail` null and puts the useful per-field messages in
// `errors`, so those are surfaced instead of the generic "One or more validation errors" title.
export function formatApiError(error: ApiError): string {
  if (error.detail) return error.detail

  const fieldMessages = error.errors ? Object.values(error.errors).flat() : []
  return fieldMessages.length > 0 ? fieldMessages.join(' ') : error.title
}
