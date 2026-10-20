import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { PendingRoute } from '@/components/common/Spinner'
import { useAuthStore } from '@/store/authStore'

export function ProtectedRoute() {
  const accessToken = useAuthStore((s) => s.accessToken)
  const isBootstrapped = useAuthStore((s) => s.isBootstrapped)
  const location = useLocation()

  if (!isBootstrapped) return <PendingRoute />
  if (!accessToken) return <Navigate to="/login" state={{ from: location }} replace />

  return <Outlet />
}
