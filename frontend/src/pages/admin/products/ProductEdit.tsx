import { type ChangeEvent, type FormEvent, useEffect, useRef, useState } from 'react'
import { Loader2, Trash2, Upload } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { TranslationTabs } from '@/components/admin/TranslationTabs'
import { SafeImage } from '@/components/common/SafeImage'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Textarea } from '@/components/ui/textarea'
import { useAdminCategories } from '@/hooks/admin/useAdminCategories'
import {
  useAdminProduct,
  useCreateProduct,
  useDeleteProductImage,
  useUpdateProduct,
  useUploadProductImage,
  useUploadProductMainImage,
} from '@/hooks/admin/useAdminProducts'
import { DiscountType, LANGUAGES, discountTypeLabels, type LanguageCode } from '@/types/enums'
import type { ProductTranslationInput, ProductUpsertRequest } from '@/types/dto'

// Kept in step with the server-side allow-list so the admin gets immediate feedback instead of
// waiting on a round trip; the API remains the authority.
const ACCEPTED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const MAX_IMAGE_BYTES = 10 * 1024 * 1024

function emptyTranslations(): Record<LanguageCode, ProductTranslationInput> {
  return Object.fromEntries(
    LANGUAGES.map((lang) => [
      lang,
      { languageCode: lang, name: '', slug: '', shortDescription: '', description: '', ingredients: '', usageInstructions: '', benefits: '', metaTitle: '', metaDescription: '' },
    ]),
  ) as Record<LanguageCode, ProductTranslationInput>
}

export function AdminProductEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const isNew = !id
  const { data: product } = useAdminProduct(id)
  const { data: categories } = useAdminCategories()
  const createProduct = useCreateProduct()
  const updateProduct = useUpdateProduct()
  const uploadImage = useUploadProductImage()
  const uploadMainImage = useUploadProductMainImage()
  const deleteImage = useDeleteProductImage()
  const mainImageInputRef = useRef<HTMLInputElement>(null)

  const [sku, setSku] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [price, setPrice] = useState('0')
  const [discountType, setDiscountType] = useState<number>(DiscountType.None)
  const [discountValue, setDiscountValue] = useState('')
  const [stock, setStock] = useState('0')
  const [mainImageUrl, setMainImageUrl] = useState('')
  const [brand, setBrand] = useState('')
  const [isFeatured, setIsFeatured] = useState(false)
  const [isBestSeller, setIsBestSeller] = useState(false)
  const [isActive, setIsActive] = useState(true)
  const [tags, setTags] = useState('')
  const [translations, setTranslations] = useState(emptyTranslations())

  useEffect(() => {
    if (!product) return
    setSku(product.sku)
    setCategoryId(product.categoryId)
    setPrice(String(product.price))
    setDiscountType(product.discountType)
    setDiscountValue(product.discountValue != null ? String(product.discountValue) : '')
    setStock(String(product.stock))
    setMainImageUrl(product.mainImageUrl ?? '')
    setBrand(product.brand ?? '')
    setIsFeatured(product.isFeatured)
    setIsBestSeller(product.isBestSeller)
    setIsActive(product.isActive)
    setTags(product.tags.join(', '))
    const next = emptyTranslations()
    for (const tr of product.translations) {
      next[tr.languageCode] = {
        languageCode: tr.languageCode,
        name: tr.name,
        slug: tr.slug,
        shortDescription: tr.shortDescription ?? '',
        description: tr.description ?? '',
        ingredients: tr.ingredients ?? '',
        usageInstructions: tr.usageInstructions ?? '',
        benefits: tr.benefits ?? '',
        metaTitle: tr.metaTitle ?? '',
        metaDescription: tr.metaDescription ?? '',
      }
    }
    setTranslations(next)
  }, [product])

  function updateTranslation(lang: LanguageCode, patch: Partial<ProductTranslationInput>) {
    setTranslations((prev) => ({ ...prev, [lang]: { ...prev[lang], ...patch } }))
  }

  function handleMainImageChange(e: ChangeEvent<HTMLInputElement>) {
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

    uploadMainImage.mutate(file, { onSuccess: (image) => setMainImageUrl(image.url) })
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()

    // Inactive tab panels are unmounted, so the `required` attributes on the English inputs
    // never fire when the admin submits from the HY or RU tab - check it explicitly.
    if (!translations.en.name.trim() || !translations.en.slug.trim()) {
      toast.error(t('admin.englishTranslationRequired'))
      return
    }

    // Languages the admin never filled in are omitted rather than sent as blank rows, so each
    // language can be entered independently instead of all three being mandatory.
    const filledTranslations = LANGUAGES.map((lang) => translations[lang]).filter(
      (tr) => tr.name.trim() !== '' || tr.slug.trim() !== '',
    )

    const body: ProductUpsertRequest = {
      sku,
      categoryId,
      price: Number(price),
      discountType: discountType as DiscountType,
      discountValue: discountValue ? Number(discountValue) : null,
      stock: Number(stock),
      mainImageUrl: mainImageUrl || null,
      brand: brand || null,
      isFeatured,
      isBestSeller,
      isActive,
      tags: tags.split(',').map((s) => s.trim()).filter(Boolean),
      translations: filledTranslations,
    }

    if (id) {
      updateProduct.mutate(
        { id, body },
        { onSuccess: () => toast.success(t('common.save')) },
      )
    } else {
      createProduct.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/products/${res.id}`)
        },
      })
    }
  }

  const isSaving = createProduct.isPending || updateProduct.isPending

  return (
    <div>
      <AdminPageHeader title={isNew ? t('admin.newProduct') : sku || t('admin.products')} />
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        <section className="grid gap-4 sm:grid-cols-3">
          <div>
            <Label className="mb-1.5">{t('admin.sku')}</Label>
            <Input value={sku} onChange={(e) => setSku(e.target.value)} required />
          </div>
          <div>
            <Label className="mb-1.5">{t('product.category')}</Label>
            <Select value={categoryId} onValueChange={setCategoryId}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder={t('common.select')} />
              </SelectTrigger>
              <SelectContent>
                {categories?.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.translations.find((tr) => tr.languageCode === 'en')?.name ?? c.id}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label className="mb-1.5">{t('product.brand')}</Label>
            <Input value={brand} onChange={(e) => setBrand(e.target.value)} />
          </div>
          <div>
            <Label className="mb-1.5">{t('common.price')}</Label>
            <Input type="number" step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
          </div>
          <div>
            <Label className="mb-1.5">{t('admin.discount')}</Label>
            <Select value={String(discountType)} onValueChange={(v) => setDiscountType(Number(v))}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(discountTypeLabels).map(([value, label]) => (
                  <SelectItem key={value} value={value}>
                    {label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          {discountType !== DiscountType.None && (
            <div>
              <Label className="mb-1.5">{t('admin.discount')} value</Label>
              <Input type="number" step="0.01" value={discountValue} onChange={(e) => setDiscountValue(e.target.value)} />
            </div>
          )}
          <div>
            <Label className="mb-1.5">{t('admin.stock')}</Label>
            <Input type="number" value={stock} onChange={(e) => setStock(e.target.value)} required />
          </div>
          <div className="sm:col-span-2">
            <Label className="mb-1.5">{t('admin.mainImage')}</Label>
            <div className="flex items-start gap-3">
              <div className="size-24 shrink-0 overflow-hidden rounded-md border bg-muted">
                <SafeImage src={mainImageUrl} alt="" />
              </div>
              <div className="flex flex-col items-start gap-2">
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={uploadMainImage.isPending}
                    onClick={() => mainImageInputRef.current?.click()}
                  >
                    {uploadMainImage.isPending ? (
                      <Loader2 className="size-4 animate-spin" />
                    ) : (
                      <Upload className="size-4" />
                    )}
                    {uploadMainImage.isPending
                      ? t('common.uploading')
                      : mainImageUrl
                        ? t('admin.replaceImage')
                        : t('admin.chooseImage')}
                  </Button>
                  {mainImageUrl && !uploadMainImage.isPending && (
                    <Button type="button" variant="ghost" size="sm" onClick={() => setMainImageUrl('')}>
                      {t('common.remove')}
                    </Button>
                  )}
                </div>
                <p className="text-xs text-muted-foreground">{t('admin.imageUploadHint')}</p>
              </div>
              <input
                ref={mainImageInputRef}
                type="file"
                accept={ACCEPTED_IMAGE_TYPES.join(',')}
                className="hidden"
                onChange={handleMainImageChange}
              />
            </div>
          </div>
          <div className="sm:col-span-3">
            <Label className="mb-1.5">{t('admin.tags')}</Label>
            <Input value={tags} onChange={(e) => setTags(e.target.value)} />
          </div>
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={isFeatured} onCheckedChange={(v) => setIsFeatured(!!v)} />
            {t('admin.featured')}
          </label>
          <label className="flex items-center gap-2 text-sm">
            <Checkbox checked={isBestSeller} onCheckedChange={(v) => setIsBestSeller(!!v)} />
            {t('admin.bestSeller')}
          </label>
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
                  <Label className="mb-1.5">{t('product.description')} ({t('common.optional')})</Label>
                  <Textarea
                    rows={3}
                    value={translations[lang].shortDescription ?? ''}
                    onChange={(e) => updateTranslation(lang, { shortDescription: e.target.value })}
                  />
                </div>
                <div className="sm:col-span-2">
                  <Label className="mb-1.5">{t('product.description')}</Label>
                  <Textarea
                    rows={5}
                    value={translations[lang].description ?? ''}
                    onChange={(e) => updateTranslation(lang, { description: e.target.value })}
                  />
                </div>
                <div>
                  <Label className="mb-1.5">{t('product.ingredients')}</Label>
                  <Textarea
                    rows={3}
                    value={translations[lang].ingredients ?? ''}
                    onChange={(e) => updateTranslation(lang, { ingredients: e.target.value })}
                  />
                </div>
                <div>
                  <Label className="mb-1.5">{t('product.usage')}</Label>
                  <Textarea
                    rows={3}
                    value={translations[lang].usageInstructions ?? ''}
                    onChange={(e) => updateTranslation(lang, { usageInstructions: e.target.value })}
                  />
                </div>
                <div className="sm:col-span-2">
                  <Label className="mb-1.5">{t('product.benefits')}</Label>
                  <Textarea
                    rows={3}
                    value={translations[lang].benefits ?? ''}
                    onChange={(e) => updateTranslation(lang, { benefits: e.target.value })}
                  />
                </div>
              </div>
            )}
          </TranslationTabs>
        </section>

        {id && product && (
          <section>
            <h2 className="mb-3 font-medium">{t('admin.images')}</h2>
            <div className="flex flex-wrap gap-3">
              {product.images.map((img) => (
                <div key={img.id} className="relative size-24 overflow-hidden rounded-md border">
                  <SafeImage src={img.url} alt={img.altText ?? ''} />
                  <button
                    type="button"
                    onClick={() => deleteImage.mutate({ id, imageId: img.id })}
                    className="absolute top-1 right-1 flex size-6 items-center justify-center rounded-full bg-background/80"
                  >
                    <Trash2 className="size-3.5" />
                  </button>
                </div>
              ))}
              <label className="flex size-24 cursor-pointer flex-col items-center justify-center gap-1 rounded-md border border-dashed text-muted-foreground hover:border-foreground">
                <Upload className="size-5" />
                <span className="text-xs">{t('common.upload')}</span>
                <input
                  type="file"
                  accept="image/*"
                  className="hidden"
                  onChange={(e) => {
                    const file = e.target.files?.[0]
                    if (file) uploadImage.mutate({ id, file })
                    e.target.value = ''
                  }}
                />
              </label>
            </div>
          </section>
        )}

        <div>
          <Button type="submit" size="lg" disabled={isSaving}>
            {isSaving ? t('common.saving') : t('common.save')}
          </Button>
        </div>
      </form>
    </div>
  )
}
