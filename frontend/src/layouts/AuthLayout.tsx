import { Link, Outlet } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

export function AuthLayout() {
  const { t } = useTranslation()
  return (
    <div className="flex min-h-screen items-center justify-center bg-secondary/40 px-4">
      <div className="w-full max-w-md">
        <Link to="/" className="mb-8 block text-center font-serif text-2xl">
          {t('brand.name')}
        </Link>
        <div className="rounded-lg border bg-card p-8 shadow-xs">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
