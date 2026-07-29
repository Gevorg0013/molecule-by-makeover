import { Badge, type badgeVariants } from '@/components/ui/badge'
import { OrderStatus, PaymentStatus, orderStatusLabels, paymentStatusLabels } from '@/types/enums'
import type { VariantProps } from 'class-variance-authority'

type BadgeVariant = VariantProps<typeof badgeVariants>['variant']

const orderStatusVariant: Record<OrderStatus, BadgeVariant> = {
  [OrderStatus.Pending]: 'secondary',
  [OrderStatus.Processing]: 'accent',
  [OrderStatus.Shipped]: 'accent',
  [OrderStatus.Delivered]: 'success',
  [OrderStatus.Completed]: 'success',
  [OrderStatus.Cancelled]: 'destructive',
}

const paymentStatusVariant: Record<PaymentStatus, BadgeVariant> = {
  [PaymentStatus.Pending]: 'secondary',
  [PaymentStatus.Paid]: 'success',
  [PaymentStatus.Failed]: 'destructive',
  [PaymentStatus.Refunded]: 'outline',
}

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return <Badge variant={orderStatusVariant[status]}>{orderStatusLabels[status]}</Badge>
}

export function PaymentStatusBadge({ status }: { status: PaymentStatus }) {
  return <Badge variant={paymentStatusVariant[status]}>{paymentStatusLabels[status]}</Badge>
}
