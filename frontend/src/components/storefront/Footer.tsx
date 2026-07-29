import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useSubscribeNewsletter } from '@/hooks/useNewsletter'
import { authErrorMessage } from '@/hooks/useAuth'

export function Footer() {
  const { t } = useTranslation()
  const [email, setEmail] = useState('')
  const subscribe = useSubscribeNewsletter()

  return (
    <footer className="mt-24 border-t bg-secondary/40">
      <div className="mx-auto grid max-w-7xl gap-10 px-4 py-12 sm:px-6 md:grid-cols-3">
        <div>
          <p className="font-serif text-lg">{t('brand.name')}</p>
          <p className="mt-2 max-w-xs text-sm text-muted-foreground">{t('brand.tagline')}</p>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <p className="mb-1 font-medium">{t('footer.about')}</p>
          <Link to="/pages/about-us" className="text-muted-foreground hover:text-foreground">
            {t('footer.about')}
          </Link>
          <Link to="/pages/privacy-policy" className="text-muted-foreground hover:text-foreground">
            {t('footer.privacy')}
          </Link>
          <Link to="/pages/terms" className="text-muted-foreground hover:text-foreground">
            {t('footer.terms')}
          </Link>
          <Link to="/pages/faq" className="text-muted-foreground hover:text-foreground">
            {t('footer.faq')}
          </Link>
        </div>

        <div>
          <p className="font-medium">{t('home.newsletterTitle')}</p>
          <p className="mt-1 text-sm text-muted-foreground">{t('home.newsletterSubtitle')}</p>
          <form
            className="mt-3 flex gap-2"
            onSubmit={(e) => {
              e.preventDefault()
              subscribe.mutate(email, {
                onSuccess: () => {
                  toast.success(t('home.newsletterTitle'))
                  setEmail('')
                },
                onError: (err) => toast.error(authErrorMessage(err)),
              })
            }}
          >
            <Input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder={t('common.email')}
            />
            <Button type="submit" disabled={subscribe.isPending}>
              {t('common.submit')}
            </Button>
          </form>
        </div>
      </div>

      <div className="border-t px-4 py-4 text-center text-xs text-muted-foreground sm:px-6">
        © {new Date().getFullYear()} {t('brand.name')} — {t('footer.rights')}
      </div>
    </footer>
  )
}
