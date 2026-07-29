import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/StatusBadge'
import { Separator } from '@/components/ui/separator'
import { useOrder } from '@/hooks/useOrders'
import { formatCurrency } from '@/lib/currency'
import { formatDateTime } from '@/lib/date'

export function AccountOrderDetailPage() {
  const { t } = useTranslation()
  const { orderNumber } = useParams()
  const { data: order, isLoading } = useOrder(orderNumber)

  if (isLoading) return <div className="px-4 py-20 text-center text-muted-foreground">{t('common.loading')}</div>
  if (!order) return null

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="font-serif text-2xl">{order.orderNumber}</h1>
          <p className="text-sm text-muted-foreground">{formatDateTime(order.placedAt)}</p>
        </div>
        <div className="flex gap-2">
          <OrderStatusBadge status={order.status} />
          <PaymentStatusBadge status={order.paymentStatus} />
        </div>
      </div>

      <h2 className="mb-2 font-medium">{t('account.items')}</h2>
      <ul className="flex flex-col divide-y">
        {order.items.map((item) => (
          <li key={item.id} className="flex justify-between py-3 text-sm">
            <span>
              {item.productName} × {item.quantity}
            </span>
            <span>{formatCurrency(item.lineTotal, order.currency)}</span>
          </li>
        ))}
      </ul>

      <Separator className="my-6" />

      <div className="ml-auto flex max-w-xs flex-col gap-2 text-sm">
        <div className="flex justify-between">
          <span className="text-muted-foreground">{t('cart.subtotal')}</span>
          <span>{formatCurrency(order.subTotal, order.currency)}</span>
        </div>
        {order.discountTotal > 0 && (
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t('cart.discount')}</span>
            <span>-{formatCurrency(order.discountTotal, order.currency)}</span>
          </div>
        )}
        {order.shippingTotal > 0 && (
          <div className="flex justify-between">
            <span className="text-muted-foreground">Shipping</span>
            <span>{formatCurrency(order.shippingTotal, order.currency)}</span>
          </div>
        )}
        <div className="flex justify-between font-medium">
          <span>{t('cart.total')}</span>
          <span>{formatCurrency(order.grandTotal, order.currency)}</span>
        </div>
      </div>

      <Separator className="my-6" />

      <h2 className="mb-2 font-medium">{t('account.shippingAddress')}</h2>
      <p className="text-sm text-muted-foreground">
        {order.shippingAddress.fullName}
        <br />
        {order.shippingAddress.addressLine1}
        {order.shippingAddress.addressLine2 && <>, {order.shippingAddress.addressLine2}</>}
        <br />
        {order.shippingAddress.city}, {order.shippingAddress.country} {order.shippingAddress.postalCode}
        <br />
        {order.shippingAddress.phone}
      </p>
    </div>
  )
}
