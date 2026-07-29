import { useState } from 'react'
import { Minus, Plus, ShoppingBag, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { SafeImage } from '@/components/common/SafeImage'
import { EmptyState } from '@/components/common/EmptyState'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import {
  useApplyCoupon,
  useCart,
  useRemoveCartItem,
  useRemoveCoupon,
  useUpdateCartItem,
} from '@/hooks/useCart'
import { formatCurrency } from '@/lib/currency'
import { useAuthStore } from '@/store/authStore'

export function CartPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const isAuthed = !!useAuthStore((s) => s.accessToken)
  const { data: cart, isLoading } = useCart()
  const updateItem = useUpdateCartItem()
  const removeItem = useRemoveCartItem()
  const applyCoupon = useApplyCoupon()
  const removeCoupon = useRemoveCoupon()
  const [coupon, setCoupon] = useState('')

  if (isLoading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
        <Skeleton className="mb-8 h-8 w-40" />
        <div className="grid gap-10 md:grid-cols-[1fr_320px]">
          <div className="flex flex-col gap-4">
            {Array.from({ length: 2 }, (_, i) => (
              <div key={i} className="flex gap-4">
                <Skeleton className="size-20 rounded-md" />
                <div className="flex flex-1 flex-col gap-2">
                  <Skeleton className="h-4 w-1/2" />
                  <Skeleton className="h-4 w-1/4" />
                </div>
              </div>
            ))}
          </div>
          <Skeleton className="h-48 rounded-lg" />
        </div>
      </div>
    )
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="mx-auto max-w-md px-4 py-16">
        <h1 className="mb-4 font-serif text-2xl">{t('cart.title')}</h1>
        <EmptyState
          icon={ShoppingBag}
          title={t('cart.empty')}
          action={
            <Button asChild>
              <Link to="/products">{t('cart.startShopping')}</Link>
            </Button>
          }
        />
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
      <h1 className="mb-8 font-serif text-3xl">{t('cart.title')}</h1>
      <div className="grid gap-10 md:grid-cols-[1fr_320px]">
        <ul className="flex flex-col divide-y">
          {cart.items.map((item) => (
            <li key={item.id} className="flex gap-4 py-4">
              <div className="size-20 shrink-0 overflow-hidden rounded-md bg-muted">
                <SafeImage src={item.imageUrl} alt={item.productName} />
              </div>
              <div className="flex flex-1 flex-col gap-1">
                <div className="flex items-start justify-between">
                  <Link to={`/products/${item.slug}`} className="font-medium hover:underline">
                    {item.productName}
                  </Link>
                  <button onClick={() => removeItem.mutate(item.id)} className="text-muted-foreground hover:text-foreground">
                    <X className="size-4" />
                  </button>
                </div>
                <span className="text-sm text-muted-foreground">{formatCurrency(item.unitPrice, cart.currency)}</span>
                <div className="mt-2 flex items-center gap-3">
                  <div className="flex items-center rounded-md border">
                    <button
                      className="flex size-7 items-center justify-center disabled:opacity-40"
                      disabled={item.quantity <= 1}
                      onClick={() => updateItem.mutate({ itemId: item.id, quantity: item.quantity - 1 })}
                    >
                      <Minus className="size-3" />
                    </button>
                    <span className="w-7 text-center text-sm">{item.quantity}</span>
                    <button
                      className="flex size-7 items-center justify-center disabled:opacity-40"
                      disabled={item.quantity >= item.availableStock}
                      onClick={() => updateItem.mutate({ itemId: item.id, quantity: item.quantity + 1 })}
                    >
                      <Plus className="size-3" />
                    </button>
                  </div>
                  <span className="font-medium">{formatCurrency(item.lineTotal, cart.currency)}</span>
                </div>
              </div>
            </li>
          ))}
        </ul>

        <div className="h-fit rounded-lg border p-5">
          <h2 className="mb-4 font-medium">{t('checkout.orderSummary')}</h2>
          <div className="flex flex-col gap-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">{t('cart.subtotal')}</span>
              <span>{formatCurrency(cart.subTotal, cart.currency)}</span>
            </div>
            {cart.discountTotal > 0 && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">{t('cart.discount')}</span>
                <span>-{formatCurrency(cart.discountTotal, cart.currency)}</span>
              </div>
            )}
          </div>

          <div className="mt-4">
            {cart.couponCode ? (
              <div className="flex items-center justify-between rounded-md bg-secondary px-3 py-2 text-sm">
                <span>{cart.couponCode}</span>
                <button onClick={() => removeCoupon.mutate()} className="text-muted-foreground hover:text-foreground">
                  {t('cart.removeCoupon')}
                </button>
              </div>
            ) : (
              <div className="flex gap-2">
                <Input
                  placeholder={t('cart.couponCode')}
                  value={coupon}
                  onChange={(e) => setCoupon(e.target.value)}
                />
                <Button
                  variant="outline"
                  disabled={!coupon || applyCoupon.isPending}
                  onClick={() =>
                    applyCoupon.mutate(coupon, {
                      onSuccess: () => {
                        toast.success(t('cart.couponApplied'))
                        setCoupon('')
                      },
                    })
                  }
                >
                  {t('cart.applyCoupon')}
                </Button>
              </div>
            )}
          </div>

          <Separator className="my-4" />
          <div className="flex justify-between text-lg font-medium">
            <span>{t('cart.total')}</span>
            <span>{formatCurrency(cart.grandTotal, cart.currency)}</span>
          </div>

          <Button
            size="lg"
            className="mt-5 w-full"
            onClick={() => navigate(isAuthed ? '/checkout' : '/login', { state: { from: { pathname: '/checkout' } } })}
          >
            {t('cart.checkout')}
          </Button>
        </div>
      </div>
    </div>
  )
}
