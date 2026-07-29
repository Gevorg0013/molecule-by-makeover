import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { isAdmin, useAuthStore } from '@/store/authStore'
import { ForbiddenPage } from '@/pages/Forbidden'

export function AdminRoute() {
  const accessToken = useAuthStore((s) => s.accessToken)
  const user = useAuthStore((s) => s.user)
  const isBootstrapped = useAuthStore((s) => s.isBootstrapped)
  const location = useLocation()

  if (!isBootstrapped) return null
  if (!accessToken) return <Navigate to="/login" state={{ from: location }} replace />
  if (!isAdmin(user)) return <ForbiddenPage />

  return <Outlet />
}
