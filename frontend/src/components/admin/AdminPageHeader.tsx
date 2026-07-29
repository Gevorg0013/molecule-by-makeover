import type { ReactNode } from 'react'

export function AdminPageHeader({ title, actions }: { title: string; actions?: ReactNode }) {
  return (
    <div className="mb-6 flex items-center justify-between">
      <h1 className="font-serif text-2xl">{title}</h1>
      {actions}
    </div>
  )
}
