import { useTranslation } from 'react-i18next'

import { AccountNav } from './AccountNav'
import { useAuthStore } from '@/store/authStore'

export function AccountProfilePage() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <h1 className="mb-6 font-serif text-3xl">{t('account.title')}</h1>
      <AccountNav />
      <div className="mt-6 grid max-w-sm gap-4 text-sm">
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('auth.firstName')}</span>
          <span>{user?.firstName}</span>
        </div>
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('auth.lastName')}</span>
          <span>{user?.lastName}</span>
        </div>
        <div className="flex justify-between border-b pb-2">
          <span className="text-muted-foreground">{t('common.email')}</span>
          <span>{user?.email}</span>
        </div>
        {user?.phoneNumber && (
          <div className="flex justify-between border-b pb-2">
            <span className="text-muted-foreground">{t('common.phone')}</span>
            <span>{user.phoneNumber}</span>
          </div>
        )}
      </div>
    </div>
  )
}
