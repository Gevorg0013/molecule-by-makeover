import { lazy } from 'react'
import { createBrowserRouter } from 'react-router-dom'

import { AdminLayout } from '@/layouts/AdminLayout'
import { AuthLayout } from '@/layouts/AuthLayout'
import { StorefrontLayout } from '@/layouts/StorefrontLayout'
import { AccountOrderDetailPage } from '@/pages/account/OrderDetail'
import { AccountOrdersPage } from '@/pages/account/Orders'
import { AccountProfilePage } from '@/pages/account/Profile'
import { BlogPage } from '@/pages/Blog'
import { BlogPostPage } from '@/pages/BlogPost'
import { CartPage } from '@/pages/Cart'
import { CheckoutCancelPage } from '@/pages/CheckoutCancel'
import { CheckoutPage } from '@/pages/Checkout'
import { CheckoutSuccessPage } from '@/pages/CheckoutSuccess'
import { ForbiddenPage } from '@/pages/Forbidden'
import { HomePage } from '@/pages/Home'
import { LoginPage } from '@/pages/Login'
import { NotFoundPage } from '@/pages/NotFound'
import { ProductDetailPage } from '@/pages/ProductDetail'
import { ProductListPage } from '@/pages/ProductList'
import { RegisterPage } from '@/pages/Register'
import { StaticPagePage } from '@/pages/StaticPage'
import { WishlistPage } from '@/pages/Wishlist'
import { AdminRoute } from './AdminRoute'
import { ProtectedRoute } from './ProtectedRoute'

const AdminDashboardPage = lazy(() => import('@/pages/admin/Dashboard').then((m) => ({ default: m.AdminDashboardPage })))
const AdminProductListPage = lazy(() => import('@/pages/admin/products/ProductList').then((m) => ({ default: m.AdminProductListPage })))
const AdminProductEditPage = lazy(() => import('@/pages/admin/products/ProductEdit').then((m) => ({ default: m.AdminProductEditPage })))
const AdminCategoryListPage = lazy(() => import('@/pages/admin/categories/CategoryList').then((m) => ({ default: m.AdminCategoryListPage })))
const AdminCategoryEditPage = lazy(() => import('@/pages/admin/categories/CategoryEdit').then((m) => ({ default: m.AdminCategoryEditPage })))
const AdminOrderListPage = lazy(() => import('@/pages/admin/orders/OrderList').then((m) => ({ default: m.AdminOrderListPage })))
const AdminOrderDetailPage = lazy(() => import('@/pages/admin/orders/OrderDetail').then((m) => ({ default: m.AdminOrderDetailPage })))
const AdminCouponListPage = lazy(() => import('@/pages/admin/coupons/CouponList').then((m) => ({ default: m.AdminCouponListPage })))
const AdminCouponEditPage = lazy(() => import('@/pages/admin/coupons/CouponEdit').then((m) => ({ default: m.AdminCouponEditPage })))
const AdminCustomerListPage = lazy(() => import('@/pages/admin/customers/CustomerList').then((m) => ({ default: m.AdminCustomerListPage })))
const AdminCustomerDetailPage = lazy(() => import('@/pages/admin/customers/CustomerDetail').then((m) => ({ default: m.AdminCustomerDetailPage })))
const AdminReviewsPage = lazy(() => import('@/pages/admin/Reviews').then((m) => ({ default: m.AdminReviewsPage })))
const AdminBannerListPage = lazy(() => import('@/pages/admin/banners/BannerList').then((m) => ({ default: m.AdminBannerListPage })))
const AdminBannerEditPage = lazy(() => import('@/pages/admin/banners/BannerEdit').then((m) => ({ default: m.AdminBannerEditPage })))
const AdminBlogListPage = lazy(() => import('@/pages/admin/blog/BlogList').then((m) => ({ default: m.AdminBlogListPage })))
const AdminBlogEditPage = lazy(() => import('@/pages/admin/blog/BlogEdit').then((m) => ({ default: m.AdminBlogEditPage })))
const AdminPageListPage = lazy(() => import('@/pages/admin/pages/PageList').then((m) => ({ default: m.AdminPageListPage })))
const AdminPageEditPage = lazy(() => import('@/pages/admin/pages/PageEdit').then((m) => ({ default: m.AdminPageEditPage })))
const AdminMediaPage = lazy(() => import('@/pages/admin/Media').then((m) => ({ default: m.AdminMediaPage })))
const AdminSettingsPage = lazy(() => import('@/pages/admin/Settings').then((m) => ({ default: m.AdminSettingsPage })))

export const router = createBrowserRouter([
  {
    element: <StorefrontLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/products', element: <ProductListPage /> },
      { path: '/category/:categorySlug', element: <ProductListPage /> },
      { path: '/products/:slug', element: <ProductDetailPage /> },
      { path: '/cart', element: <CartPage /> },
      { path: '/wishlist', element: <ProtectedRoute /> , children: [{ index: true, element: <WishlistPage /> }] },
      { path: '/blog', element: <BlogPage /> },
      { path: '/blog/:slug', element: <BlogPostPage /> },
      { path: '/pages/:key', element: <StaticPagePage /> },
      { path: '/checkout/success', element: <CheckoutSuccessPage /> },
      { path: '/checkout/cancel', element: <CheckoutCancelPage /> },
      {
        element: <ProtectedRoute />,
        children: [
          { path: '/checkout', element: <CheckoutPage /> },
          { path: '/account', element: <AccountProfilePage /> },
          { path: '/account/orders', element: <AccountOrdersPage /> },
          { path: '/account/orders/:orderNumber', element: <AccountOrderDetailPage /> },
        ],
      },
      { path: '/forbidden', element: <ForbiddenPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  {
    element: <AuthLayout />,
    children: [
      { path: '/login', element: <LoginPage /> },
      { path: '/register', element: <RegisterPage /> },
    ],
  },
  {
    path: '/admin',
    element: <AdminRoute />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { index: true, element: <AdminDashboardPage /> },
          { path: 'products', element: <AdminProductListPage /> },
          { path: 'products/new', element: <AdminProductEditPage /> },
          { path: 'products/:id', element: <AdminProductEditPage /> },
          { path: 'categories', element: <AdminCategoryListPage /> },
          { path: 'categories/new', element: <AdminCategoryEditPage /> },
          { path: 'categories/:id', element: <AdminCategoryEditPage /> },
          { path: 'orders', element: <AdminOrderListPage /> },
          { path: 'orders/:orderNumber', element: <AdminOrderDetailPage /> },
          { path: 'coupons', element: <AdminCouponListPage /> },
          { path: 'coupons/new', element: <AdminCouponEditPage /> },
          { path: 'coupons/:id', element: <AdminCouponEditPage /> },
          { path: 'customers', element: <AdminCustomerListPage /> },
          { path: 'customers/:id', element: <AdminCustomerDetailPage /> },
          { path: 'reviews', element: <AdminReviewsPage /> },
          { path: 'banners', element: <AdminBannerListPage /> },
          { path: 'banners/new', element: <AdminBannerEditPage /> },
          { path: 'banners/:id', element: <AdminBannerEditPage /> },
          { path: 'blog', element: <AdminBlogListPage /> },
          { path: 'blog/new', element: <AdminBlogEditPage /> },
          { path: 'blog/:id', element: <AdminBlogEditPage /> },
          { path: 'pages', element: <AdminPageListPage /> },
          { path: 'pages/new', element: <AdminPageEditPage /> },
          { path: 'pages/:id', element: <AdminPageEditPage /> },
          { path: 'media', element: <AdminMediaPage /> },
          { path: 'settings', element: <AdminSettingsPage /> },
        ],
      },
    ],
  },
])
