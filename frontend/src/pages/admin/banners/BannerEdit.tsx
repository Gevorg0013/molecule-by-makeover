import { type FormEvent, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { TranslationTabs } from '@/components/admin/TranslationTabs'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useAdminBanners, useCreateBanner, useUpdateBanner } from '@/hooks/admin/useAdminBanners'
import { LANGUAGES, type LanguageCode } from '@/types/enums'
import type { BannerTranslationInput, BannerUpsertRequest } from '@/types/dto'

function emptyTranslations(): Record<LanguageCode, BannerTranslationInput> {
  return Object.fromEntries(
    LANGUAGES.map((lang) => [lang, { languageCode: lang, title: '', subtitle: '', ctaText: '' }]),
  ) as Record<LanguageCode, BannerTranslationInput>
}

export function AdminBannerEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const { data: banners } = useAdminBanners()
  const banner = banners?.find((b) => b.id === id)
  const createBanner = useCreateBanner()
  const updateBanner = useUpdateBanner()

  const [imageUrl, setImageUrl] = useState('')
  const [linkUrl, setLinkUrl] = useState('')
  const [sortOrder, setSortOrder] = useState('0')
  const [isActive, setIsActive] = useState(true)
  const [startsAt, setStartsAt] = useState('')
  const [endsAt, setEndsAt] = useState('')
  const [translations, setTranslations] = useState(emptyTranslations())

  useEffect(() => {
    if (!banner) return
    setImageUrl(banner.imageUrl)
    setLinkUrl(banner.linkUrl ?? '')
    setSortOrder(String(banner.sortOrder))
    setIsActive(banner.isActive)
    setStartsAt(banner.startsAt ? banner.startsAt.slice(0, 10) : '')
    setEndsAt(banner.endsAt ? banner.endsAt.slice(0, 10) : '')
    const next = emptyTranslations()
    for (const tr of banner.translations) {
      next[tr.languageCode] = {
        languageCode: tr.languageCode,
        title: tr.title ?? '',
        subtitle: tr.subtitle ?? '',
        ctaText: tr.ctaText ?? '',
      }
    }
    setTranslations(next)
  }, [banner])

  function updateTranslation(lang: LanguageCode, patch: Partial<BannerTranslationInput>) {
    setTranslations((prev) => ({ ...prev, [lang]: { ...prev[lang], ...patch } }))
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body: BannerUpsertRequest = {
      imageUrl,
      linkUrl: linkUrl || null,
      sortOrder: Number(sortOrder),
      isActive,
      startsAt: startsAt ? new Date(startsAt).toISOString() : null,
      endsAt: endsAt ? new Date(endsAt).toISOString() : null,
      translations: LANGUAGES.map((lang) => translations[lang]),
    }
    if (id) {
      updateBanner.mutate({ id, body }, { onSuccess: () => toast.success(t('common.save')) })
    } else {
      createBanner.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/banners/${res.id}`)
        },
      })
    }
  }

  const isSaving = createBanner.isPending || updateBanner.isPending

  return (
    <div>
      <AdminPageHeader title={id ? t('common.edit') : t('admin.newBanner')} />
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        <section className="grid gap-4 sm:grid-cols-3">
          <div className="sm:col-span-2">
            <Label className="mb-1.5">Image URL</Label>
            <Input value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} required />
          </div>
          <div>
            <Label className="mb-1.5">Sort Order</Label>
            <Input type="number" value={sortOrder} onChange={(e) => setSortOrder(e.target.value)} />
          </div>
          <div className="sm:col-span-2">
            <Label className="mb-1.5">Link URL</Label>
            <Input value={linkUrl} onChange={(e) => setLinkUrl(e.target.value)} />
          </div>
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={isActive} onCheckedChange={(v) => setIsActive(!!v)} />
            {t('common.active')}
          </label>
          <div>
            <Label className="mb-1.5">Starts At</Label>
            <Input type="date" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} />
          </div>
          <div>
            <Label className="mb-1.5">Ends At</Label>
            <Input type="date" value={endsAt} onChange={(e) => setEndsAt(e.target.value)} />
          </div>
        </section>

        <section>
          <h2 className="mb-3 font-medium">{t('admin.translations')}</h2>
          <TranslationTabs>
            {(lang) => (
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <Label className="mb-1.5">Title</Label>
                  <Input
                    value={translations[lang].title ?? ''}
                    onChange={(e) => updateTranslation(lang, { title: e.target.value })}
                  />
                </div>
                <div>
                  <Label className="mb-1.5">Subtitle</Label>
                  <Input
                    value={translations[lang].subtitle ?? ''}
                    onChange={(e) => updateTranslation(lang, { subtitle: e.target.value })}
                  />
                </div>
                <div>
                  <Label className="mb-1.5">CTA Text</Label>
                  <Input
                    value={translations[lang].ctaText ?? ''}
                    onChange={(e) => updateTranslation(lang, { ctaText: e.target.value })}
                  />
                </div>
              </div>
            )}
          </TranslationTabs>
        </section>

        <div>
          <Button type="submit" size="lg" disabled={isSaving}>
            {isSaving ? t('common.saving') : t('common.save')}
          </Button>
        </div>
      </form>
    </div>
  )
}
