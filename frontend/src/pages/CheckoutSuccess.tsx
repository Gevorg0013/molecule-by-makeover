import { CheckCircle2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useSearchParams } from 'react-router-dom'

import { Button } from '@/components/ui/button'

export function CheckoutSuccessPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const orderNumber = searchParams.get('order') ?? ''

  return (
    <div className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 py-24 text-center">
      <CheckCircle2 className="size-14 text-accent" />
      <h1 className="font-serif text-2xl">{t('checkoutResult.successTitle')}</h1>
      <p className="text-muted-foreground">{t('checkoutResult.successSubtitle', { orderNumber })}</p>
      {orderNumber && (
        <Button asChild>
          <Link to={`/account/orders/${orderNumber}`}>{t('checkoutResult.viewOrder')}</Link>
        </Button>
      )}
    </div>
  )
}
