export function formatDate(value: string | null | undefined, locale?: string): string {
  if (!value) return '—'
  return new Intl.DateTimeFormat(locale ?? 'en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(new Date(value))
}

export function formatDateTime(value: string | null | undefined, locale?: string): string {
  if (!value) return '—'
  return new Intl.DateTimeFormat(locale ?? 'en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value))
}
