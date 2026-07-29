import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { useAdminCustomer } from '@/hooks/admin/useAdminCustomers'
import { formatDate } from '@/lib/date'

export function AdminCustomerDetailPage() {
  const { t } = useTranslation()
  const { id } = useParams()
  const { data: customer, isLoading } = useAdminCustomer(id)

  if (isLoading) return <p className="text-sm text-muted-foreground">{t('common.loading')}</p>
  if (!customer) return null

  return (
    <div>
      <AdminPageHeader title={`${customer.firstName} ${customer.lastName}`} />
      <div className="grid max-w-sm gap-3 text-sm">
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('common.email')}</span>
          <span>{customer.email}</span>
        </div>
        {customer.phoneNumber && (
          <div className="flex justify-between border-b pb-2">
            <span className="text-muted-foreground">{t('common.phone')}</span>
            <span>{customer.phoneNumber}</span>
          </div>
        )}
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('admin.customerSince')}</span>
          <span>{formatDate(customer.createdAt)}</span>
        </div>
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('common.status')}</span>
          <span>{customer.isActive ? t('common.active') : t('common.inactive')}</span>
        </div>
      </div>
    </div>
  )
}
