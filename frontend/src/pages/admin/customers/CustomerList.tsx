import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminCustomers } from '@/hooks/admin/useAdminCustomers'
import { formatDate } from '@/lib/date'

export function AdminCustomerListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const { data, isLoading } = useAdminCustomers(page)

  return (
    <div>
      <AdminPageHeader title={t('admin.customers')} />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>{t('common.email')}</TableHead>
            <TableHead>{t('admin.customerSince')}</TableHead>
            <TableHead>{t('nav.orders')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data?.items.map((c) => (
            <TableRow key={c.id} className="cursor-pointer" onClick={() => navigate(`/admin/customers/${c.id}`)}>
              <TableCell>
                {c.firstName} {c.lastName}
              </TableCell>
              <TableCell>{c.email}</TableCell>
              <TableCell>{formatDate(c.createdAt)}</TableCell>
              <TableCell>{c.orderCount}</TableCell>
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
