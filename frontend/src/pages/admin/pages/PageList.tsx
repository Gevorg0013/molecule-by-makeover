import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminPagesList, useDeletePage } from '@/hooks/admin/useAdminPages'

export function AdminPageListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: pages, isLoading } = useAdminPagesList()
  const deletePage = useDeletePage()

  return (
    <div>
      <AdminPageHeader
        title={t('admin.pages')}
        actions={
          <Button asChild>
            <Link to="/admin/pages/new">
              <Plus className="size-4" /> {t('admin.newPage')}
            </Link>
          </Button>
        }
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('admin.key')}</TableHead>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {pages?.map((p) => {
            const title = p.translations.find((tr) => tr.languageCode === 'en')?.title
            return (
              <TableRow key={p.id} className="cursor-pointer" onClick={() => navigate(`/admin/pages/${p.id}`)}>
                <TableCell>{p.key}</TableCell>
                <TableCell>{title ?? '—'}</TableCell>
                <TableCell>
                  <Badge variant={p.isPublished ? 'success' : 'secondary'}>
                    {p.isPublished ? t('common.active') : t('common.inactive')}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={(e) => {
                      e.stopPropagation()
                      if (confirm(t('common.confirm') + '?')) deletePage.mutate(p.id)
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
