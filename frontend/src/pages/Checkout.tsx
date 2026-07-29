import { zodResolver } from '@hookform/resolvers/zod'
import { Elements } from '@stripe/react-stripe-js'
import { type ReactNode, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'

import { CheckoutPaymentForm } from '@/components/storefront/CheckoutPaymentForm'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import { useCart } from '@/hooks/useCart'
import { useCheckout } from '@/hooks/useCheckout'
import { authErrorMessage } from '@/hooks/useAuth'
import { formatCurrency } from '@/lib/currency'
import { stripePromise } from '@/lib/stripe'
import type { CheckoutResultDto } from '@/types/dto'

const addressSchema = z.object({
  fullName: z.string().min(1),
  phone: z.string().min(1),
  country: z.string().min(1),
  city: z.string().min(1),
  addressLine1: z.string().min(1),
  addressLine2: z.string().optional(),
  postalCode: z.string().optional(),
})

type AddressForm = z.infer<typeof addressSchema>

export function CheckoutPage() {
  const { t } = useTranslation()
  const { data: cart } = useCart()
  const checkout = useCheckout()
  const [result, setResult] = useState<CheckoutResultDto | null>(null)
  const [error, setError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AddressForm>({ resolver: zodResolver(addressSchema) })

  function onSubmit(values: AddressForm) {
    setError(null)
    checkout.mutate(
      { shippingAddress: values, paymentProviderKey: 'stripe' },
      {
        onSuccess: (res) => setResult(res),
        onError: (err) => setError(authErrorMessage(err)),
      },
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
      <h1 className="mb-8 font-serif text-3xl">{t('checkout.title')}</h1>
      <div className="grid gap-10 md:grid-cols-[1fr_320px]">
        <div>
          {!result ? (
            <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
              <h2 className="font-medium">{t('checkout.shippingAddress')}</h2>
              <div className="grid gap-4 sm:grid-cols-2">
                <Field label={t('checkout.fullName')} error={errors.fullName}>
                  <Input {...register('fullName')} />
                </Field>
                <Field label={t('common.phone')} error={errors.phone}>
                  <Input {...register('phone')} />
                </Field>
                <Field label={t('checkout.country')} error={errors.country}>
                  <Input {...register('country')} />
                </Field>
                <Field label={t('checkout.city')} error={errors.city}>
                  <Input {...register('city')} />
                </Field>
                <Field label={t('checkout.addressLine1')} error={errors.addressLine1} className="sm:col-span-2">
                  <Input {...register('addressLine1')} />
                </Field>
                <Field label={t('checkout.addressLine2')}>
                  <Input {...register('addressLine2')} />
                </Field>
                <Field label={t('checkout.postalCode')}>
                  <Input {...register('postalCode')} />
                </Field>
              </div>
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" size="lg" disabled={checkout.isPending}>
                {checkout.isPending ? t('checkout.placingOrder') : t('checkout.payment')}
              </Button>
            </form>
          ) : result.clientSecret && stripePromise ? (
            <div>
              <h2 className="mb-4 font-medium">{t('checkout.payment')}</h2>
              <Elements stripe={stripePromise} options={{ clientSecret: result.clientSecret }}>
                <CheckoutPaymentForm orderNumber={result.orderNumber} />
              </Elements>
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">{t('common.somethingWentWrong')}</p>
          )}
        </div>

        {cart && (
          <div className="h-fit rounded-lg border p-5">
            <h2 className="mb-4 font-medium">{t('checkout.orderSummary')}</h2>
            <ul className="flex flex-col gap-2 text-sm">
              {cart.items.map((item) => (
                <li key={item.id} className="flex justify-between">
                  <span className="text-muted-foreground">
                    {item.productName} × {item.quantity}
                  </span>
                  <span>{formatCurrency(item.lineTotal, cart.currency)}</span>
                </li>
              ))}
            </ul>
            <Separator className="my-4" />
            <div className="flex justify-between text-lg font-medium">
              <span>{t('cart.total')}</span>
              <span>{formatCurrency(cart.grandTotal, cart.currency)}</span>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

function Field({
  label,
  error,
  className,
  children,
}: {
  label: string
  error?: { message?: string }
  className?: string
  children: ReactNode
}) {
  return (
    <div className={className}>
      <Label className="mb-1.5">{label}</Label>
      {children}
      {error?.message && <p className="mt-1 text-xs text-destructive">{error.message}</p>}
    </div>
  )
}
