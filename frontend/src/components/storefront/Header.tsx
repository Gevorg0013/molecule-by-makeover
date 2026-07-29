import { Heart, LogOut, Menu, Package, Search, Shield, ShoppingBag, User, UserCircle } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'

import { LanguageSwitcher } from './LanguageSwitcher'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Separator } from '@/components/ui/separator'
import { Sheet, SheetContent, SheetTitle } from '@/components/ui/sheet'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useCart } from '@/hooks/useCart'
import { useLogout } from '@/hooks/useAuth'
import { isAdmin, useAuthStore } from '@/store/authStore'
import { useUiStore } from '@/store/uiStore'

const navLinks = [
  { to: '/', key: 'nav.home' },
  { to: '/products', key: 'nav.shop' },
  { to: '/blog', key: 'nav.blog' },
]

export function Header() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const logout = useLogout()
  const openCart = useUiStore((s) => s.openCart)
  const { data: cart } = useCart()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [search, setSearch] = useState('')
  const itemCount = cart?.items.reduce((sum, i) => sum + i.quantity, 0) ?? 0

  function submitSearch(e: FormEvent) {
    e.preventDefault()
    setMobileOpen(false)
    navigate(`/products?search=${encodeURIComponent(search)}`)
  }

  return (
    <header className="sticky top-0 z-40 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:rounded-md focus:bg-accent focus:px-3 focus:py-2 focus:text-accent-foreground"
      >
        Skip to content
      </a>
      <div className="mx-auto flex h-16 max-w-7xl items-center gap-4 px-4 sm:px-6">
        <Button
          variant="ghost"
          size="icon"
          className="md:hidden"
          onClick={() => setMobileOpen(true)}
          aria-label="Menu"
        >
          <Menu className="size-5" />
        </Button>

        <Link to="/" className="font-serif text-lg font-medium tracking-tight whitespace-nowrap">
          {t('brand.name')}
        </Link>

        <nav className="hidden gap-6 md:flex">
          {navLinks.map((link) => (
            <Link
              key={link.to}
              to={link.to}
              className="text-sm font-medium text-foreground/80 transition-colors hover:text-foreground"
            >
              {t(link.key)}
            </Link>
          ))}
        </nav>

        <form className="ml-auto hidden max-w-xs flex-1 items-center md:flex" onSubmit={submitSearch}>
          <div className="relative w-full">
            <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={t('common.search')}
              className="pl-8"
            />
          </div>
        </form>

        <div className="ml-auto flex items-center gap-1 md:ml-0 md:gap-2">
          <LanguageSwitcher />

          <Button variant="ghost" size="icon" asChild className="hidden sm:inline-flex">
            <Link to="/wishlist" aria-label={t('nav.wishlist')}>
              <Heart className="size-5" />
            </Link>
          </Button>

          <Button
            variant="ghost"
            size="icon"
            className="relative transition-transform active:scale-90"
            onClick={openCart}
            aria-label={t('nav.cart')}
          >
            <ShoppingBag className="size-5" />
            {itemCount > 0 && (
              <span className="absolute -top-1 -right-1 flex size-4 items-center justify-center rounded-full bg-accent text-[10px] font-medium text-accent-foreground">
                {itemCount}
              </span>
            )}
          </Button>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon" className="hidden sm:inline-flex" aria-label={t('nav.account')}>
                <User className="size-5" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {user ? (
                <>
                  <DropdownMenuItem asChild>
                    <Link to="/account">{t('account.title')}</Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link to="/account/orders">{t('nav.orders')}</Link>
                  </DropdownMenuItem>
                  {isAdmin(user) && (
                    <DropdownMenuItem asChild>
                      <Link to="/admin">{t('nav.admin')}</Link>
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onSelect={() => logout.mutate()}>{t('nav.logout')}</DropdownMenuItem>
                </>
              ) : (
                <>
                  <DropdownMenuItem asChild>
                    <Link to="/login">{t('nav.login')}</Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link to="/register">{t('nav.register')}</Link>
                  </DropdownMenuItem>
                </>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent side="left" className="w-72">
          <SheetTitle className="px-4 pt-4 font-serif text-lg">{t('brand.name')}</SheetTitle>
          <div className="flex flex-col gap-4 px-4">
            <form onSubmit={submitSearch}>
              <div className="relative">
                <Search className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder={t('common.search')}
                  className="pl-8"
                />
              </div>
            </form>

            <nav className="flex flex-col gap-1">
              {navLinks.map((link) => (
                <Link
                  key={link.to}
                  to={link.to}
                  onClick={() => setMobileOpen(false)}
                  className="rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                >
                  {t(link.key)}
                </Link>
              ))}
              <Link
                to="/wishlist"
                onClick={() => setMobileOpen(false)}
                className="flex items-center gap-2 rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
              >
                <Heart className="size-4" /> {t('nav.wishlist')}
              </Link>
            </nav>

            <Separator />

            <nav className="flex flex-col gap-1">
              {user ? (
                <>
                  <Link
                    to="/account"
                    onClick={() => setMobileOpen(false)}
                    className="flex items-center gap-2 rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                  >
                    <UserCircle className="size-4" /> {t('account.title')}
                  </Link>
                  <Link
                    to="/account/orders"
                    onClick={() => setMobileOpen(false)}
                    className="flex items-center gap-2 rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                  >
                    <Package className="size-4" /> {t('nav.orders')}
                  </Link>
                  {isAdmin(user) && (
                    <Link
                      to="/admin"
                      onClick={() => setMobileOpen(false)}
                      className="flex items-center gap-2 rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                    >
                      <Shield className="size-4" /> {t('nav.admin')}
                    </Link>
                  )}
                  <button
                    onClick={() => {
                      logout.mutate()
                      setMobileOpen(false)
                    }}
                    className="flex items-center gap-2 rounded-md px-2 py-2.5 text-left text-sm font-medium hover:bg-accent/10"
                  >
                    <LogOut className="size-4" /> {t('nav.logout')}
                  </button>
                </>
              ) : (
                <>
                  <Link
                    to="/login"
                    onClick={() => setMobileOpen(false)}
                    className="rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                  >
                    {t('nav.login')}
                  </Link>
                  <Link
                    to="/register"
                    onClick={() => setMobileOpen(false)}
                    className="rounded-md px-2 py-2.5 text-sm font-medium hover:bg-accent/10"
                  >
                    {t('nav.register')}
                  </Link>
                </>
              )}
            </nav>
          </div>
        </SheetContent>
      </Sheet>
    </header>
  )
}
