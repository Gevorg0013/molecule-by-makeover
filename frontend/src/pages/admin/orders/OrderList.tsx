import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { OrderStatusBadge, PaymentStatusBadge } from '@/components/common/StatusBadge'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminOrders } from '@/hooks/admin/useAdminOrders'
import { formatCurrency } from '@/lib/currency'
import { formatDateTime } from '@/lib/date'
import { OrderStatus, orderStatusLabels } from '@/types/enums'

const ALL = '__all__'

export function AdminOrderListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState(ALL)
  const { data, isLoading } = useAdminOrders(page, 20, status === ALL ? undefined : (Number(status) as OrderStatus))

  return (
    <div>
      <AdminPageHeader title={t('admin.orders')} />
      <Select value={status} onValueChange={(v) => { setStatus(v); setPage(1) }}>
        <SelectTrigger className="mb-4 w-48">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>{t('common.all')}</SelectItem>
          {Object.entries(orderStatusLabels).map(([value, label]) => (
            <SelectItem key={value} value={value}>
              {label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('account.orderNumber')}</TableHead>
            <TableHead>{t('common.date')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead>{t('account.paymentStatus')}</TableHead>
            <TableHead>{t('common.price')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data?.items.map((o) => (
            <TableRow key={o.id} className="cursor-pointer" onClick={() => navigate(`/admin/orders/${o.orderNumber}`)}>
              <TableCell>{o.orderNumber}</TableCell>
              <TableCell>{formatDateTime(o.placedAt)}</TableCell>
              <TableCell>
                <OrderStatusBadge status={o.status} />
              </TableCell>
              <TableCell>
                <PaymentStatusBadge status={o.paymentStatus} />
              </TableCell>
              <TableCell>{formatCurrency(o.grandTotal, o.currency)}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {data && data.totalPages > 1 && (
        <div className="mt-6 flex items-center justify-center gap-3">
          <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            {t('common.previous')}
          </Button>
          <span className="text-sm text-muted-foreground">
            {t('common.page')} {page} {t('common.of')} {data.totalPages}
          </span>
          <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>
            {t('common.next')}
          </Button>
        </div>
      )}
    </div>
  )
}
