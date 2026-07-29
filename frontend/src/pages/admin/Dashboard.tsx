import { useTranslation } from 'react-i18next'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminDashboard } from '@/hooks/admin/useAdminDashboard'
import { formatCurrency } from '@/lib/currency'

export function AdminDashboardPage() {
  const { t } = useTranslation()
  const { data, isLoading } = useAdminDashboard()

  if (isLoading || !data) return <p className="text-sm text-muted-foreground">{t('common.loading')}</p>

  const tiles = [
    { label: t('admin.totalRevenue'), value: formatCurrency(data.totalRevenue, 'AMD') },
    { label: t('admin.totalOrders'), value: data.totalOrders },
    { label: t('admin.pendingOrders'), value: data.pendingOrders },
    { label: t('admin.totalCustomers'), value: data.totalCustomers },
    { label: t('admin.totalProducts'), value: data.totalProducts },
    { label: t('admin.lowStockProducts'), value: data.lowStockProducts },
  ]

  return (
    <div>
      <AdminPageHeader title={t('admin.dashboard')} />
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {tiles.map((tile) => (
          <Card key={tile.label}>
            <CardHeader className="pb-1">
              <CardTitle className="text-xs font-normal text-muted-foreground">{tile.label}</CardTitle>
            </CardHeader>
            <CardContent className="text-xl font-semibold">{tile.value}</CardContent>
          </Card>
        ))}
      </div>

      <Card className="mt-8">
        <CardHeader>
          <CardTitle>{t('admin.topProducts')}</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t('common.name')}</TableHead>
                <TableHead>{t('admin.unitsSold')}</TableHead>
                <TableHead>{t('admin.revenue')}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.topProducts.map((p) => (
                <TableRow key={p.productId}>
                  <TableCell>{p.name}</TableCell>
                  <TableCell>{p.unitsSold}</TableCell>
                  <TableCell>{formatCurrency(p.revenue, 'AMD')}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
