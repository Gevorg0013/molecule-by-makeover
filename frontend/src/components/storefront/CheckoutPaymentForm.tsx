import { PaymentElement, useElements, useStripe } from '@stripe/react-stripe-js'
import { type FormEvent, useState } from 'react'
import { useTranslation } from 'react-i18next'

import { Button } from '@/components/ui/button'

export function CheckoutPaymentForm({ orderNumber }: { orderNumber: string }) {
  const { t } = useTranslation()
  const stripe = useStripe()
  const elements = useElements()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    if (!stripe || !elements) return
    setIsSubmitting(true)
    setError(null)

    const { error: confirmError } = await stripe.confirmPayment({
      elements,
      confirmParams: {
        return_url: `${window.location.origin}/checkout/success?order=${orderNumber}`,
      },
    })

    if (confirmError) {
      setError(confirmError.message ?? t('common.somethingWentWrong'))
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      <PaymentElement />
      {error && <p className="text-sm text-destructive">{error}</p>}
      <Button type="submit" size="lg" disabled={!stripe || isSubmitting}>
        {isSubmitting ? t('checkout.placingOrder') : t('checkout.placeOrder')}
      </Button>
    </form>
  )
}
