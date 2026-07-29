export function formatCurrency(amount: number, currency: string, locale?: string): string {
  try {
    return new Intl.NumberFormat(locale ?? 'en-US', {
      style: 'currency',
      currency,
      currencyDisplay: currency === 'AMD' ? 'code' : 'symbol',
      maximumFractionDigits: currency === 'AMD' ? 0 : 2,
    }).format(amount)
  } catch {
    return `${amount.toFixed(2)} ${currency}`
  }
}
