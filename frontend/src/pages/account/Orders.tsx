import { Package } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { AccountNav } from './AccountNav'
import { EmptyState } from '@/components/common/EmptyState'
import { OrderStatusBadge } from '@/components/common/StatusBadge'
import { Skeleton } from '@/components/ui/skeleton'
import { useOrders } from '@/hooks/useOrders'
import { formatCurrency } from '@/lib/currency'
import { formatDate } from '@/lib/date'

export function AccountOrdersPage() {
  const { t } = useTranslation()
  const { data, isLoading } = useOrders()

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <h1 className="mb-6 font-serif text-3xl">{t('account.title')}</h1>
      <AccountNav />
      <div className="mt-6">
        {isLoading && (
          <div className="flex flex-col divide-y">
            {Array.from({ length: 3 }, (_, i) => (
              <div key={i} className="flex items-center justify-between py-4">
                <div className="space-y-2">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-3 w-20" />
                </div>
                <Skeleton className="h-6 w-20" />
              </div>
            ))}
          </div>
        )}
        {!isLoading && data && data.items.length === 0 && (
          <EmptyState icon={Package} title={t('account.noOrders')} />
        )}
        <ul className="flex flex-col divide-y">
          {data?.items.map((order) => (
            <li key={order.id} className="py-4">
              <Link
                to={`/account/orders/${order.orderNumber}`}
                className="flex items-center justify-between rounded-md transition-colors hover:bg-accent/5"
              >
                <div>
                  <p className="font-medium">{order.orderNumber}</p>
                  <p className="text-xs text-muted-foreground">{formatDate(order.placedAt)}</p>
                </div>
                <div className="flex items-center gap-3">
                  <span className="text-sm">{formatCurrency(order.grandTotal, order.currency)}</span>
                  <OrderStatusBadge status={order.status} />
                </div>
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
