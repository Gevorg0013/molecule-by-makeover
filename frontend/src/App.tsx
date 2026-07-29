import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { Toaster } from 'sonner'

import '@/i18n'
import { useBootstrapAuth } from '@/hooks/useAuth'
import { queryClient } from '@/lib/queryClient'
import { router } from '@/routes/router'

function AppShell() {
  useBootstrapAuth()
  return <RouterProvider router={router} />
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppShell />
      <Toaster position="top-center" richColors />
    </QueryClientProvider>
  )
}

export default App
