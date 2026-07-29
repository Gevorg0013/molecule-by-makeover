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
import { useAdminBlogPost, useCreateBlogPost, useUpdateBlogPost } from '@/hooks/admin/useAdminBlog'
import { LANGUAGES, type LanguageCode } from '@/types/enums'
import type { BlogPostUpsertRequest, BlogTranslationInput } from '@/types/dto'

function emptyTranslations(): Record<LanguageCode, BlogTranslationInput> {
  return Object.fromEntries(
    LANGUAGES.map((lang) => [lang, { languageCode: lang, title: '', slug: '', excerpt: '', content: '', metaTitle: '', metaDescription: '' }]),
  ) as Record<LanguageCode, BlogTranslationInput>
}

export function AdminBlogEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const { data: post } = useAdminBlogPost(id)
  const createPost = useCreateBlogPost()
  const updatePost = useUpdateBlogPost()

  const [coverImageUrl, setCoverImageUrl] = useState('')
  const [isPublished, setIsPublished] = useState(false)
  const [translations, setTranslations] = useState(emptyTranslations())

  useEffect(() => {
    if (!post) return
    setCoverImageUrl(post.coverImageUrl ?? '')
    setIsPublished(post.isPublished)
    const next = emptyTranslations()
    for (const tr of post.translations) {
      next[tr.languageCode] = {
        languageCode: tr.languageCode,
        title: tr.title,
        slug: tr.slug,
        excerpt: tr.excerpt ?? '',
        content: tr.content,
        metaTitle: tr.metaTitle ?? '',
        metaDescription: tr.metaDescription ?? '',
      }
    }
    setTranslations(next)
  }, [post])

  function updateTranslation(lang: LanguageCode, patch: Partial<BlogTranslationInput>) {
    setTranslations((prev) => ({ ...prev, [lang]: { ...prev[lang], ...patch } }))
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body: BlogPostUpsertRequest = {
      coverImageUrl: coverImageUrl || null,
      isPublished,
      translations: LANGUAGES.map((lang) => translations[lang]),
    }
    if (id) {
      updatePost.mutate({ id, body }, { onSuccess: () => toast.success(t('common.save')) })
    } else {
      createPost.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/blog/${res.id}`)
        },
      })
    }
  }

  const isSaving = createPost.isPending || updatePost.isPending

  return (
    <div>
      <AdminPageHeader title={id ? t('common.edit') : t('admin.newPost')} />
      <form onSubmit={handleSubmit} className="flex flex-col gap-8">
        <section className="grid gap-4 sm:grid-cols-2">
          <div>
            <Label className="mb-1.5">Cover Image URL</Label>
            <Input value={coverImageUrl} onChange={(e) => setCoverImageUrl(e.target.value)} />
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
                  <Label className="mb-1.5">Excerpt</Label>
                  <Textarea
                    rows={2}
                    value={translations[lang].excerpt ?? ''}
                    onChange={(e) => updateTranslation(lang, { excerpt: e.target.value })}
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
