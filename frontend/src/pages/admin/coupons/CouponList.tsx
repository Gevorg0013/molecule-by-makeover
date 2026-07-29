import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminCoupons, useDeleteCoupon } from '@/hooks/admin/useAdminCoupons'
import { formatCurrency } from '@/lib/currency'
import { DiscountType } from '@/types/enums'

export function AdminCouponListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: coupons, isLoading } = useAdminCoupons()
  const deleteCoupon = useDeleteCoupon()

  return (
    <div>
      <AdminPageHeader
        title={t('admin.coupons')}
        actions={
          <Button asChild>
            <Link to="/admin/coupons/new">
              <Plus className="size-4" /> {t('admin.newCoupon')}
            </Link>
          </Button>
        }
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('admin.code')}</TableHead>
            <TableHead>{t('admin.discount')}</TableHead>
            <TableHead>Uses</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {coupons?.map((c) => (
            <TableRow key={c.id} className="cursor-pointer" onClick={() => navigate(`/admin/coupons/${c.id}`)}>
              <TableCell>{c.code}</TableCell>
              <TableCell>
                {c.discountType === DiscountType.Percent ? `${c.discountValue}%` : formatCurrency(c.discountValue, 'AMD')}
              </TableCell>
              <TableCell>
                {c.usesCount}
                {c.maxUses ? ` / ${c.maxUses}` : ''}
              </TableCell>
              <TableCell>
                <Badge variant={c.isActive ? 'success' : 'secondary'}>
                  {c.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
              </TableCell>
              <TableCell className="text-right">
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={(e) => {
                    e.stopPropagation()
                    if (confirm(t('common.confirm') + '?')) deleteCoupon.mutate(c.id)
                  }}
                >
                  <Trash2 className="size-4" />
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
