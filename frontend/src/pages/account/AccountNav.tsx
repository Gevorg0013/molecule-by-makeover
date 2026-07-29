import { useTranslation } from 'react-i18next'
import { NavLink } from 'react-router-dom'

import { cn } from '@/lib/utils'

export function AccountNav() {
  const { t } = useTranslation()
  const items = [
    { to: '/account', end: true, key: 'account.profile' },
    { to: '/account/orders', end: false, key: 'account.orderHistory' },
    { to: '/wishlist', end: false, key: 'wishlist.title' },
  ]
  return (
    <nav className="flex gap-1 border-b">
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.end}
          className={({ isActive }) =>
            cn(
              'border-b-2 px-3 py-2 text-sm font-medium',
              isActive ? 'border-foreground text-foreground' : 'border-transparent text-muted-foreground hover:text-foreground',
            )
          }
        >
          {t(item.key)}
        </NavLink>
      ))}
    </nav>
  )
}
