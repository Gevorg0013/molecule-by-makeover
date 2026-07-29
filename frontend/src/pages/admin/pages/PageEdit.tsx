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
import { Textarea } from '@/components/ui/textarea'
import { useAdminPage, useCreatePage, useUpdatePage } from '@/hooks/admin/useAdminPages'
import { LANGUAGES, type LanguageCode } from '@/types/enums'
import type { PageTranslationInput, PageUpsertRequest } from '@/types/dto'

function emptyTranslations(): Record<LanguageCode, PageTranslationInput> {
  return Object.fromEntries(
    LANGUAGES.map((lang) => [lang, { languageCode: lang, title: '', slug: '', content: '', metaTitle: '', metaDescription: '' }]),
  ) as Record<LanguageCode, PageTranslationInput>
}

export function AdminPageEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const { data: page } = useAdminPage(id)
  const createPage = useCreatePage()
  const updatePage = useUpdatePage()

  const [key, setKey] = useState('')
  const [isPublished, setIsPublished] = useState(false)
  const [translations, setTranslations] = useState(emptyTranslations())

  useEffect(() => {
    if (!page) return
    setKey(page.key)
    setIsPublished(page.isPublished)
    const next = emptyTranslations()
    for (const tr of page.translations) {
      next[tr.languageCode] = {
        languageCode: tr.languageCode,
        title: tr.title,
        slug: tr.slug,
        content: tr.content,
        metaTitle: tr.metaTitle ?? '',
        metaDescription: tr.metaDescription ?? '',
      }
    }
    setTranslations(next)
  }, [page])

  function updateTranslation(lang: LanguageCode, patch: Partial<PageTranslationInput>) {
    setTranslations((prev) => ({ ...prev, [lang]: { ...prev[lang], ...patch } }))
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body: PageUpsertRequest = {
      key,
      isPublished,
      translations: LANGUAGES.map((lang) => translations[lang]),
    }
    if (id) {
      updatePage.mutate({ id, body }, { onSuccess: () => toast.success(t('common.save')) })
    } else {
      createPage.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/pages/${res.id}`)
        },
      })
    }
  }

  const isSaving = createPage.isPending || updatePage.isPending

  return (
    <div>
      <AdminPageHeader title={id ? t('common.edit') : t('admin.newPage')} />
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        <section className="grid gap-4 sm:grid-cols-2">
          <div>
            <Label className="mb-1.5">{t('admin.key')}</Label>
            <Input value={key} onChange={(e) => setKey(e.target.value)} placeholder="about-us" required />
          </div>
          <label className="flex items-center gap-2 self-end pb-2 text-sm">
            <Checkbox checked={isPublished} onCheckedChange={(v) => setIsPublished(!!v)} />
            {t('common.active')}
          </label>
        </section>

        <section>
          <h2 className="mb-3 font-medium">{t('admin.translations')}</h2>
          <TranslationTabs>
            {(lang) => (
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <Label className="mb-1.5">Title</Label>
                  <Input
                    value={translations[lang].title}
                    onChange={(e) => updateTranslation(lang, { title: e.target.value })}
                    required={lang === 'en'}
                  />
                </div>
                <div>
                  <Label className="mb-1.5">Slug</Label>
                  <Input
                    value={translations[lang].slug}
                    onChange={(e) => updateTranslation(lang, { slug: e.target.value })}
                    required={lang === 'en'}
                  />
                </div>
                <div className="sm:col-span-2">
                  <Label className="mb-1.5">Content (HTML)</Label>
                  <Textarea
                    rows={10}
                    value={translations[lang].content}
                    onChange={(e) => updateTranslation(lang, { content: e.target.value })}
                    required={lang === 'en'}
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
