import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminProducts, useDeleteProduct } from '@/hooks/admin/useAdminProducts'
import { formatCurrency } from '@/lib/currency'

export function AdminProductListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: products, isLoading } = useAdminProducts()
  const deleteProduct = useDeleteProduct()
  const [search, setSearch] = useState('')

  const filtered = products?.filter((p) => {
    const name = p.translations.find((tr) => tr.languageCode === 'en')?.name ?? ''
    return name.toLowerCase().includes(search.toLowerCase()) || p.sku.toLowerCase().includes(search.toLowerCase())
  })

  return (
    <div>
      <AdminPageHeader
        title={t('admin.products')}
        actions={
          <Button asChild>
            <Link to="/admin/products/new">
              <Plus className="size-4" /> {t('admin.newProduct')}
            </Link>
          </Button>
        }
      />
      <Input
        placeholder={t('common.search')}
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="mb-4 max-w-xs"
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('admin.sku')}</TableHead>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>{t('common.price')}</TableHead>
            <TableHead>{t('admin.stock')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filtered?.map((p) => {
            const name = p.translations.find((tr) => tr.languageCode === 'en')?.name ?? p.sku
            return (
              <TableRow key={p.id} className="cursor-pointer" onClick={() => navigate(`/admin/products/${p.id}`)}>
                <TableCell>{p.sku}</TableCell>
                <TableCell>{name}</TableCell>
                <TableCell>{formatCurrency(p.price, 'AMD')}</TableCell>
                <TableCell>{p.stock}</TableCell>
                <TableCell>
                  <Badge variant={p.isActive ? 'success' : 'secondary'}>
                    {p.isActive ? t('common.active') : t('common.inactive')}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={(e) => {
                      e.stopPropagation()
                      if (confirm(t('common.confirm') + '?')) deleteProduct.mutate(p.id)
                    }}
                  >
                    <Trash2 className="size-4" />
                  </Button>
                </TableCell>
              </TableRow>
            )
          })}
        </TableBody>
      </Table>
    </div>
  )
}
