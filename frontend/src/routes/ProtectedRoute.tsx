import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAuthStore } from '@/store/authStore'

export function ProtectedRoute() {
  const accessToken = useAuthStore((s) => s.accessToken)
  const isBootstrapped = useAuthStore((s) => s.isBootstrapped)
  const location = useLocation()

  if (!isBootstrapped) return null
  if (!accessToken) return <Navigate to="/login" state={{ from: location }} replace />

  return <Outlet />
}
