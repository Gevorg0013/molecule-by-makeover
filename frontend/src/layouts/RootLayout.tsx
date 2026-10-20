import { Outlet } from 'react-router-dom'

import { RouteProgress } from '@/components/common/RouteProgress'

// Pathless wrapper around every route so app-wide chrome that needs router context
// (the top progress bar) is mounted exactly once.
export function RootLayout() {
  return (
    <>
      <RouteProgress />
      <Outlet />
    </>
  )
}
