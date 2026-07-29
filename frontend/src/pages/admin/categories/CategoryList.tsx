import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminCategories, useDeleteCategory } from '@/hooks/admin/useAdminCategories'

export function AdminCategoryListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: categories, isLoading } = useAdminCategories()
  const deleteCategory = useDeleteCategory()

  return (
    <div>
      <AdminPageHeader
        title={t('admin.categories')}
        actions={
          <Button asChild>
            <Link to="/admin/categories/new">
              <Plus className="size-4" /> {t('admin.newCategory')}
            </Link>
          </Button>
        }
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>{t('admin.parentCategory')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {categories?.map((c) => {
            const name = c.translations.find((tr) => tr.languageCode === 'en')?.name ?? c.id
            const parent = categories.find((p) => p.id === c.parentCategoryId)
            const parentName = parent?.translations.find((tr) => tr.languageCode === 'en')?.name
            return (
              <TableRow key={c.id} className="cursor-pointer" onClick={() => navigate(`/admin/categories/${c.id}`)}>
                <TableCell>{name}</TableCell>
                <TableCell>{parentName ?? t('admin.none')}</TableCell>
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
                      if (confirm(t('common.confirm') + '?')) deleteCategory.mutate(c.id)
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
