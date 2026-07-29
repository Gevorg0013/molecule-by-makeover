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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import {
  useAdminCategories,
  useAdminCategory,
  useCreateCategory,
  useUpdateCategory,
} from '@/hooks/admin/useAdminCategories'
import { LANGUAGES, type LanguageCode } from '@/types/enums'
import type { CategoryTranslationInput, CategoryUpsertRequest } from '@/types/dto'

const NONE = '__none__'

function emptyTranslations(): Record<LanguageCode, CategoryTranslationInput> {
  return Object.fromEntries(
    LANGUAGES.map((lang) => [lang, { languageCode: lang, name: '', slug: '', description: '', metaTitle: '', metaDescription: '' }]),
  ) as Record<LanguageCode, CategoryTranslationInput>
}

export function AdminCategoryEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const { data: category } = useAdminCategory(id)
  const { data: categories } = useAdminCategories()
  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()

  const [parentCategoryId, setParentCategoryId] = useState(NONE)
  const [imageUrl, setImageUrl] = useState('')
  const [sortOrder, setSortOrder] = useState('0')
  const [isActive, setIsActive] = useState(true)
  const [translations, setTranslations] = useState(emptyTranslations())

  useEffect(() => {
    if (!category) return
    setParentCategoryId(category.parentCategoryId ?? NONE)
    setImageUrl(category.imageUrl ?? '')
    setSortOrder(String(category.sortOrder))
    setIsActive(category.isActive)
    const next = emptyTranslations()
    for (const tr of category.translations) {
      next[tr.languageCode] = {
        languageCode: tr.languageCode,
        name: tr.name,
        slug: tr.slug,
        description: tr.description ?? '',
        metaTitle: tr.metaTitle ?? '',
        metaDescription: tr.metaDescription ?? '',
      }
    }
    setTranslations(next)
  }, [category])

  function updateTranslation(lang: LanguageCode, patch: Partial<CategoryTranslationInput>) {
    setTranslations((prev) => ({ ...prev, [lang]: { ...prev[lang], ...patch } }))
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body: CategoryUpsertRequest = {
      parentCategoryId: parentCategoryId === NONE ? null : parentCategoryId,
      imageUrl: imageUrl || null,
      sortOrder: Number(sortOrder),
      isActive,
      translations: LANGUAGES.map((lang) => translations[lang]),
    }
    if (id) {
      updateCategory.mutate({ id, body }, { onSuccess: () => toast.success(t('common.save')) })
    } else {
      createCategory.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/categories/${res.id}`)
        },
      })
    }
  }

  const isSaving = createCategory.isPending || updateCategory.isPending

  return (
    <div>
      <AdminPageHeader title={id ? t('common.edit') : t('admin.newCategory')} />
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        <section className="grid gap-4 sm:grid-cols-3">
          <div>
            <Label className="mb-1.5">{t('admin.parentCategory')}</Label>
            <Select value={parentCategoryId} onValueChange={setParentCategoryId}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={NONE}>{t('admin.none')}</SelectItem>
                {categories?.filter((c) => c.id !== id).map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.translations.find((tr) => tr.languageCode === 'en')?.name ?? c.id}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label className="mb-1.5">Image URL</Label>
            <Input value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} />
          </div>
          <div>
            <Label className="mb-1.5">Sort Order</Label>
            <Input type="number" value={sortOrder} onChange={(e) => setSortOrder(e.target.value)} />
          </div>
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={isActive} onCheckedChange={(v) => setIsActive(!!v)} />
            {t('common.active')}
          </label>
        </section>

        <section>
          <h2 className="mb-3 font-medium">{t('admin.translations')}</h2>
          <TranslationTabs>
            {(lang) => (
              <div className="grid gap-4 sm:grid-cols-2">
                <div>
                  <Label className="mb-1.5">{t('common.name')}</Label>
                  <Input
                    value={translations[lang].name}
                    onChange={(e) => updateTranslation(lang, { name: e.target.value })}
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
                  <Label className="mb-1.5">{t('product.description')}</Label>
                  <Textarea
                    rows={3}
                    value={translations[lang].description ?? ''}
                    onChange={(e) => updateTranslation(lang, { description: e.target.value })}
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
