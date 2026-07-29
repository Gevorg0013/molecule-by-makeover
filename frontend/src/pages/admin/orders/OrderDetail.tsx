import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/StatusBadge'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { useAdminOrder, useUpdateOrderStatus } from '@/hooks/admin/useAdminOrders'
import { formatCurrency } from '@/lib/currency'
import { formatDateTime } from '@/lib/date'
import { orderStatusLabels, orderStatusTransitions, type OrderStatus } from '@/types/enums'

export function AdminOrderDetailPage() {
  const { t } = useTranslation()
  const { orderNumber } = useParams()
  const { data: order, isLoading } = useAdminOrder(orderNumber)
  const updateStatus = useUpdateOrderStatus()

  if (isLoading) return <p className="text-sm text-muted-foreground">{t('common.loading')}</p>
  if (!order) return null

  const nextStatuses = orderStatusTransitions[order.status]

  return (
    <div>
      <AdminPageHeader title={order.orderNumber} />
      <div className="mb-6 flex flex-wrap items-center gap-3">
        <OrderStatusBadge status={order.status} />
        <PaymentStatusBadge status={order.paymentStatus} />
        <span className="text-sm text-muted-foreground">{formatDateTime(order.placedAt)}</span>
      </div>

      {nextStatuses.length > 0 && (
        <div className="mb-8 flex flex-wrap items-center gap-2">
          <span className="text-sm text-muted-foreground">{t('admin.updateStatus')}:</span>
          {nextStatuses.map((status: OrderStatus) => (
            <Button
              key={status}
              size="sm"
              variant="outline"
              disabled={updateStatus.isPending}
              onClick={() => updateStatus.mutate({ id: order.id, status })}
            >
              {orderStatusLabels[status]}
            </Button>
          ))}
        </div>
      )}

      <h2 className="mb-2 font-medium">{t('account.items')}</h2>
      <ul className="flex flex-col divide-y">
        {order.items.map((item) => (
          <li key={item.id} className="flex justify-between py-3 text-sm">
            <span>
              {item.productName} ({item.sku}) × {item.quantity}
            </span>
            <span>{formatCurrency(item.lineTotal, order.currency)}</span>
          </li>
        ))}
      </ul>

      <Separator className="my-6" />

      <div className="grid gap-8 sm:grid-cols-2">
        <div>
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
        <div className="flex flex-col gap-2 text-sm">
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t('cart.subtotal')}</span>
            <span>{formatCurrency(order.subTotal, order.currency)}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-muted-foreground">{t('cart.discount')}</span>
            <span>-{formatCurrency(order.discountTotal, order.currency)}</span>
          </div>
          <div className="flex justify-between font-medium">
            <span>{t('cart.total')}</span>
            <span>{formatCurrency(order.grandTotal, order.currency)}</span>
          </div>
        </div>
      </div>
    </div>
  )
}
