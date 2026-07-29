import { Minus, Plus, ShoppingBag, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { EmptyState } from '@/components/common/EmptyState'
import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetFooter, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { Separator } from '@/components/ui/separator'
import { Skeleton } from '@/components/ui/skeleton'
import { useCart, useRemoveCartItem, useUpdateCartItem } from '@/hooks/useCart'
import { useUiStore } from '@/store/uiStore'
import { formatCurrency } from '@/lib/currency'

export function CartDrawer() {
  const { t } = useTranslation()
  const isOpen = useUiStore((s) => s.isCartOpen)
  const closeCart = useUiStore((s) => s.closeCart)
  const { data: cart, isLoading } = useCart()
  const updateItem = useUpdateCartItem()
  const removeItem = useRemoveCartItem()

  return (
    <Sheet open={isOpen} onOpenChange={(open) => !open && closeCart()}>
      <SheetContent className="w-full sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{t('cart.title')}</SheetTitle>
        </SheetHeader>

        <div className="flex-1 overflow-y-auto px-4">
          {isLoading && (
            <div className="flex flex-col gap-4 py-2">
              {Array.from({ length: 2 }, (_, i) => (
                <div key={i} className="flex gap-3">
                  <Skeleton className="size-16 shrink-0 rounded-md" />
                  <div className="flex-1 space-y-2">
                    <Skeleton className="h-4 w-3/4" />
                    <Skeleton className="h-3 w-1/3" />
                  </div>
                </div>
              ))}
            </div>
          )}
          {!isLoading && (!cart || cart.items.length === 0) && (
            <EmptyState
              icon={ShoppingBag}
              title={t('cart.empty')}
              action={
                <Button asChild onClick={closeCart}>
                  <Link to="/products">{t('cart.startShopping')}</Link>
                </Button>
              }
            />
          )}
          {cart && cart.items.length > 0 && (
            <ul className="flex flex-col gap-4">
              {cart.items.map((item) => (
                <li key={item.id} className="flex gap-3">
                  <div className="size-16 shrink-0 overflow-hidden rounded-md bg-muted">
                    <SafeImage src={item.imageUrl} alt={item.productName} />
                  </div>
                  <div className="flex flex-1 flex-col gap-1">
                    <div className="flex items-start justify-between gap-2">
                      <Link
                        to={`/products/${item.slug}`}
                        onClick={closeCart}
                        className="text-sm font-medium hover:underline"
                      >
                        {item.productName}
                      </Link>
                      <button
                        aria-label={t('cart.removeItem')}
                        onClick={() => removeItem.mutate(item.id)}
                        className="text-muted-foreground hover:text-foreground"
                      >
                        <X className="size-4" />
                      </button>
                    </div>
                    <span className="text-sm text-muted-foreground">
                      {formatCurrency(item.unitPrice, cart.currency)}
                    </span>
                    <div className="mt-1 flex items-center gap-2">
                      <button
                        className="flex size-6 items-center justify-center rounded border disabled:opacity-40"
                        disabled={item.quantity <= 1}
                        onClick={() => updateItem.mutate({ itemId: item.id, quantity: item.quantity - 1 })}
                      >
                        <Minus className="size-3" />
                      </button>
                      <span className="w-6 text-center text-sm">{item.quantity}</span>
                      <button
                        className="flex size-6 items-center justify-center rounded border disabled:opacity-40"
                        disabled={item.quantity >= item.availableStock}
                        onClick={() => updateItem.mutate({ itemId: item.id, quantity: item.quantity + 1 })}
                      >
                        <Plus className="size-3" />
                      </button>
                      <span className="ml-auto text-sm font-medium">
                        {formatCurrency(item.lineTotal, cart.currency)}
                      </span>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>

        {cart && cart.items.length > 0 && (
          <SheetFooter>
            <Separator />
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">{t('cart.subtotal')}</span>
              <span>{formatCurrency(cart.subTotal, cart.currency)}</span>
            </div>
            {cart.discountTotal > 0 && (
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">{t('cart.discount')}</span>
                <span>-{formatCurrency(cart.discountTotal, cart.currency)}</span>
              </div>
            )}
            <div className="flex justify-between font-medium">
              <span>{t('cart.total')}</span>
              <span>{formatCurrency(cart.grandTotal, cart.currency)}</span>
            </div>
            <Button asChild size="lg" onClick={closeCart}>
              <Link to="/cart">{t('cart.checkout')}</Link>
            </Button>
          </SheetFooter>
        )}
      </SheetContent>
    </Sheet>
  )
}
