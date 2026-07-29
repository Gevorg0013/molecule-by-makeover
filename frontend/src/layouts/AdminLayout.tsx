import {
  Image,
  LayoutDashboard,
  LogOut,
  Menu,
  Newspaper,
  Package,
  Settings,
  ShoppingCart,
  Star,
  Tags,
  Ticket,
  Users,
} from 'lucide-react'
import { Suspense, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, NavLink, Outlet } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetTitle } from '@/components/ui/sheet'
import { cn } from '@/lib/utils'
import { useLogout } from '@/hooks/useAuth'
import { useAuthStore } from '@/store/authStore'

const navItems = [
  { to: '/admin', end: true, icon: LayoutDashboard, key: 'admin.dashboard' },
  { to: '/admin/products', icon: Package, key: 'admin.products' },
  { to: '/admin/categories', icon: Tags, key: 'admin.categories' },
  { to: '/admin/orders', icon: ShoppingCart, key: 'admin.orders' },
  { to: '/admin/coupons', icon: Ticket, key: 'admin.coupons' },
  { to: '/admin/customers', icon: Users, key: 'admin.customers' },
  { to: '/admin/reviews', icon: Star, key: 'admin.reviews' },
  { to: '/admin/banners', icon: Image, key: 'admin.banners' },
  { to: '/admin/blog', icon: Newspaper, key: 'admin.blog' },
  { to: '/admin/pages', icon: Newspaper, key: 'admin.pages' },
  { to: '/admin/media', icon: Image, key: 'admin.media' },
  { to: '/admin/settings', icon: Settings, key: 'admin.settings' },
]

function AdminNav({ onNavigate }: { onNavigate?: () => void }) {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()

  return (
    <div className="flex h-full flex-col">
      <Link to="/" className="px-5 py-5 font-serif text-lg">
        {t('brand.name')}
      </Link>
      <nav className="flex flex-1 flex-col gap-0.5 px-2">
        {navItems.map(({ to, end, icon: Icon, key }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            onClick={onNavigate}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-2.5 rounded-md px-3 py-2 text-sm transition-colors',
                isActive
                  ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                  : 'text-sidebar-foreground/70 hover:bg-sidebar-accent/60 hover:text-sidebar-accent-foreground',
              )
            }
          >
            <Icon className="size-4" />
            {t(key)}
          </NavLink>
        ))}
      </nav>
      <div className="border-t border-sidebar-border px-3 py-3">
        <p className="truncate px-1 text-xs text-sidebar-foreground/60">{user?.email}</p>
        <button
          onClick={() => logout.mutate()}
          className="mt-1 flex w-full items-center gap-2.5 rounded-md px-3 py-2 text-sm text-sidebar-foreground/70 hover:bg-sidebar-accent/60"
        >
          <LogOut className="size-4" />
          {t('admin.logout')}
        </button>
      </div>
    </div>
  )
}

export function AdminLayout() {
  const { t } = useTranslation()
  const [mobileNavOpen, setMobileNavOpen] = useState(false)

  return (
    <div className="flex min-h-screen bg-background">
      <aside className="hidden w-60 shrink-0 flex-col bg-sidebar text-sidebar-foreground md:flex">
        <AdminNav />
      </aside>

      <Sheet open={mobileNavOpen} onOpenChange={setMobileNavOpen}>
        <SheetContent side="left" className="w-64 border-sidebar-border bg-sidebar p-0 text-sidebar-foreground">
          <SheetTitle className="sr-only">{t('admin.dashboard')}</SheetTitle>
          <AdminNav onNavigate={() => setMobileNavOpen(false)} />
        </SheetContent>
      </Sheet>

      <div className="min-w-0 flex-1">
        <header className="flex items-center gap-3 border-b px-4 py-3 md:hidden">
          <Button variant="ghost" size="icon" onClick={() => setMobileNavOpen(true)} aria-label={t('admin.dashboard')}>
            <Menu className="size-5" />
          </Button>
          <Link to="/" className="font-serif text-base">
            {t('brand.name')}
          </Link>
        </header>
        <main className="mx-auto max-w-6xl p-4 sm:p-6">
          <Suspense fallback={<p className="text-sm text-muted-foreground">{t('common.loading')}</p>}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  )
}
