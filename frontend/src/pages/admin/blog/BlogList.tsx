import { Plus, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminBlogList, useDeleteBlogPost } from '@/hooks/admin/useAdminBlog'
import { formatDate } from '@/lib/date'

export function AdminBlogListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: posts, isLoading } = useAdminBlogList()
  const deletePost = useDeleteBlogPost()

  return (
    <div>
      <AdminPageHeader
        title={t('admin.blog')}
        actions={
          <Button asChild>
            <Link to="/admin/blog/new">
              <Plus className="size-4" /> {t('admin.newPost')}
            </Link>
          </Button>
        }
      />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>{t('common.name')}</TableHead>
            <TableHead>{t('common.date')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {posts?.map((p) => {
            const title = p.translations.find((tr) => tr.languageCode === 'en')?.title
            return (
              <TableRow key={p.id} className="cursor-pointer" onClick={() => navigate(`/admin/blog/${p.id}`)}>
                <TableCell>{title ?? '—'}</TableCell>
                <TableCell>{formatDate(p.publishedAt)}</TableCell>
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
                      if (confirm(t('common.confirm') + '?')) deletePost.mutate(p.id)
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
