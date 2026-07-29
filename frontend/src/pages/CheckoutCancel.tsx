import { XCircle } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { Button } from '@/components/ui/button'

export function CheckoutCancelPage() {
  const { t } = useTranslation()
  return (
    <div className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 py-24 text-center">
      <XCircle className="size-14 text-destructive" />
      <h1 className="font-serif text-2xl">{t('checkoutResult.cancelTitle')}</h1>
      <p className="text-muted-foreground">{t('checkoutResult.cancelSubtitle')}</p>
      <Button asChild>
        <Link to="/cart">{t('cart.title')}</Link>
      </Button>
    </div>
  )
}
