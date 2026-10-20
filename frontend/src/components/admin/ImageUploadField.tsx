import { type ChangeEvent, useRef } from 'react'
import { Loader2, Upload } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'

import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { cn } from '@/lib/utils'
import type { GalleryImageDto } from '@/types/dto'

// Kept in step with the server-side allow-list so the admin gets immediate feedback instead of
// waiting on a round trip; the API remains the authority.
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_IMAGE_BYTES = 10 * 1024 * 1024

/** The slice of a TanStack upload mutation this field needs. */
interface ImageUploader {
  mutate: (file: File, options: { onSuccess: (image: GalleryImageDto) => void }) => void
  isPending: boolean
}

interface ImageUploadFieldProps {
  label: string
  value: string
  onChange: (url: string) => void
  uploader: ImageUploader
  /** Offer a Remove button; leave off for fields the form requires. */
  removable?: boolean
  /** Preview box size/shape, e.g. a wide box for banners. */
  previewClassName?: string
  className?: string
}

/**
 * Upload-to-URL field for admin forms: picks a file, validates it client-side, uploads it, and
 * hands the stored URL back via `onChange` so the form persists it with its upsert payload.
 */
export function ImageUploadField({
  label,
  value,
  onChange,
  uploader,
  removable = true,
  previewClassName = 'size-24',
  className,
}: ImageUploadFieldProps) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement>(null)

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    e.target.value = ''
    if (!file) return

    if (!ACCEPTED_IMAGE_TYPES.includes(file.type)) {
      toast.error(t('admin.invalidImageType'))
      return
    }
    if (file.size === 0) {
      toast.error(t('admin.emptyImage'))
      return
    }
    if (file.size > MAX_IMAGE_BYTES) {
      toast.error(t('admin.imageTooLarge'))
      return
    }

    uploader.mutate(file, { onSuccess: (image) => onChange(image.url) })
  }

  return (
    <div className={className}>
      <Label className="mb-1.5">{label}</Label>
      <div className="flex items-start gap-3">
        <div className={cn('shrink-0 overflow-hidden rounded-md border bg-muted', previewClassName)}>
          <SafeImage src={value} alt="" />
        </div>
        <div className="flex flex-col items-start gap-2">
          <div className="flex flex-wrap items-center gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={uploader.isPending}
              onClick={() => inputRef.current?.click()}
            >
              {uploader.isPending ? <Loader2 className="size-4 animate-spin" /> : <Upload className="size-4" />}
              {uploader.isPending
                ? t('common.uploading')
                : value
                  ? t('admin.replaceImage')
                  : t('admin.chooseImage')}
            </Button>
            {removable && value && !uploader.isPending && (
              <Button type="button" variant="ghost" size="sm" onClick={() => onChange('')}>
                {t('common.remove')}
              </Button>
            )}
          </div>
          <p className="text-xs text-muted-foreground">{t('admin.imageUploadHint')}</p>
        </div>
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPTED_IMAGE_TYPES.join(',')}
          className="hidden"
          onChange={handleChange}
        />
      </div>
    </div>
  )
}
