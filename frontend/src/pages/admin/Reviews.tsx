import { Trash2 } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { RatingStars } from '@/components/storefront/RatingStars'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { useAdminReviews, useApproveReview, useDeleteReview } from '@/hooks/admin/useAdminReviews'
import { formatDate } from '@/lib/date'

const ALL = '__all__'

export function AdminReviewsPage() {
  const { t } = useTranslation()
  const [filter, setFilter] = useState(ALL)
  const approvedOnly = filter === ALL ? undefined : filter === 'true'
  const { data: reviews, isLoading } = useAdminReviews(approvedOnly)
  const approve = useApproveReview()
  const remove = useDeleteReview()

  return (
    <div>
      <AdminPageHeader title={t('admin.reviews')} />
      <Select value={filter} onValueChange={setFilter}>
        <SelectTrigger className="mb-4 w-48">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value={ALL}>{t('common.all')}</SelectItem>
          <SelectItem value="true">{t('admin.approved')}</SelectItem>
          <SelectItem value="false">{t('admin.pendingApproval')}</SelectItem>
        </SelectContent>
      </Select>

      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Reviewer</TableHead>
            <TableHead>{t('product.rating')}</TableHead>
            <TableHead>{t('product.comment')}</TableHead>
            <TableHead>{t('common.date')}</TableHead>
            <TableHead>{t('common.status')}</TableHead>
            <TableHead className="text-right">{t('common.actions')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {reviews?.map((r) => (
            <TableRow key={r.id}>
              <TableCell>{r.reviewerName}</TableCell>
              <TableCell>
                <RatingStars rating={r.rating} />
              </TableCell>
              <TableCell className="max-w-xs truncate">{r.comment}</TableCell>
              <TableCell>{formatDate(r.createdAt)}</TableCell>
              <TableCell>
                <Badge variant={r.isApproved ? 'success' : 'secondary'}>
                  {r.isApproved ? t('admin.approved') : t('admin.pendingApproval')}
                </Badge>
              </TableCell>
              <TableCell className="flex justify-end gap-1 text-right">
                {!r.isApproved && (
                  <Button size="sm" variant="outline" onClick={() => approve.mutate(r.id)}>
                    {t('admin.approve')}
                  </Button>
                )}
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => {
                    if (confirm(t('common.confirm') + '?')) remove.mutate(r.id)
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
