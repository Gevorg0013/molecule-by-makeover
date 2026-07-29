import { Trash2, Upload } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { SafeImage } from '@/components/common/SafeImage'
import { useAdminMedia, useDeleteMedia, useUploadMedia } from '@/hooks/admin/useAdminMedia'
import { formatDate } from '@/lib/date'

export function AdminMediaPage() {
  const { t } = useTranslation()
  const { data: media, isLoading } = useAdminMedia()
  const upload = useUploadMedia()
  const remove = useDeleteMedia()

  return (
    <div>
      <AdminPageHeader title={t('admin.mediaLibrary')} />
      {isLoading && <p className="text-sm text-muted-foreground">{t('common.loading')}</p>}
      <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
        <label className="flex aspect-square cursor-pointer flex-col items-center justify-center gap-1 rounded-md border border-dashed text-center text-muted-foreground hover:border-foreground">
          <Upload className="size-6" />
          <span className="px-2 text-xs">{t('admin.dragDropUpload')}</span>
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp,image/gif"
            className="hidden"
            onChange={(e) => {
              const file = e.target.files?.[0]
              if (file) {
                upload.mutate(
                  { file },
                  { onSuccess: () => toast.success(t('common.upload')) },
                )
              }
              e.target.value = ''
            }}
          />
        </label>
        {media?.map((img) => (
          <div key={img.id} className="group relative aspect-square overflow-hidden rounded-md border">
            <SafeImage src={img.url} alt={img.altText ?? img.fileName} />
            <div className="absolute inset-0 flex flex-col justify-between bg-black/0 p-1.5 opacity-0 transition-opacity group-hover:bg-black/40 group-hover:opacity-100">
              <button
                onClick={() => {
                  if (confirm(t('common.confirm') + '?')) remove.mutate(img.id)
                }}
                className="ml-auto flex size-6 items-center justify-center rounded-full bg-background/90"
              >
                <Trash2 className="size-3.5" />
              </button>
              <span className="truncate rounded bg-background/90 px-1 py-0.5 text-[10px]">
                {formatDate(img.createdAt)}
              </span>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
