import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useSubscribeNewsletter } from '@/hooks/useNewsletter'
import { authErrorMessage } from '@/hooks/useAuth'

// lucide-react 1.x ships no brand icons, so these are inline outline glyphs drawn in the same
// 24px / 2px-stroke style as the rest of the site's icons.
const SOCIAL_LINKS = [
  {
    label: 'Instagram',
    href: 'https://www.instagram.com/makeover.beautyroom?cplk=MXQxNHc2dTZmc2gxOQ==',
    icon: (
      <>
        <rect x="2" y="2" width="20" height="20" rx="5" ry="5" />
        <path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z" />
        <line x1="17.5" x2="17.51" y1="6.5" y2="6.5" />
      </>
    ),
  },
  {
    label: 'Facebook',
    href: 'https://facebook.com/share/1UYdmiTkkj?mibextid=wwXIfr',
    icon: <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z" />,
  },
]

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
          <div className="-ml-2 mt-4 flex items-center gap-1">
            {SOCIAL_LINKS.map((link) => (
              <a
                key={link.label}
                href={link.href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={link.label}
                title={link.label}
                className="inline-flex size-10 items-center justify-center rounded-full text-muted-foreground transition-colors hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
              >
                <svg
                  viewBox="0 0 24 24"
                  className="size-5"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  aria-hidden="true"
                >
                  {link.icon}
                </svg>
              </a>
            ))}
          </div>
        </div>

        <div className="flex flex-col gap-2 text-sm">
          <p className="mb-1 font-medium">{t('footer.about')}</p>
          <Link to="/pages/about" className="text-muted-foreground hover:text-foreground">
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
