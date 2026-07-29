import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { SafeImage } from '@/components/common/SafeImage'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminBanners, useDeleteBanner } from '@/hooks/admin/useAdminBanners'

export function AdminBannerListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: banners, isLoading } = useAdminBanners()
  const deleteBanner = useDeleteBanner()

  return (
    <div>
      <AdminPageHeader
        title={t('admin.banners')}
        actions={
          <Button asChild>
            <Link to="/admin/banners/new">
              <Plus className="size-4" /> {t('admin.newBanner')}
            </Link>
          </Button>
        }
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Image</TableHead>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>Sort</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {banners?.map((b) => {
            const title = b.translations.find((tr) => tr.languageCode === 'en')?.title
            return (
              <TableRow key={b.id} className="cursor-pointer" onClick={() => navigate(`/admin/banners/${b.id}`)}>
                <TableCell>
                  <div className="h-10 w-16 overflow-hidden rounded bg-muted">
                    <SafeImage src={b.imageUrl} alt="" />
                  </div>
                </TableCell>
                <TableCell>{title ?? '—'}</TableCell>
                <TableCell>{b.sortOrder}</TableCell>
                <TableCell>
                  <Badge variant={b.isActive ? 'success' : 'secondary'}>
                    {b.isActive ? t('common.active') : t('common.inactive')}
                  </Badge>
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={(e) => {
                      e.stopPropagation()
                      if (confirm(t('common.confirm') + '?')) deleteBanner.mutate(b.id)
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
