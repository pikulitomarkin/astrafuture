'use client'

import { useAuth } from '@/hooks/use-auth'

interface HeaderProps {
  title: string
  description?: string
}

export function Header({ title, description }: HeaderProps) {
  const { user } = useAuth()

  return (
    <div className="border-b border-gray-200 bg-white px-8 py-4 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-[#075E54]">{title}</h1>
          {description && (
            <p className="text-[#333333] mt-1 text-sm">{description}</p>
          )}
        </div>
        <div className="flex items-center space-x-4">
          <div className="text-right">
            <p className="text-sm text-[#333333]">{user?.businessName ? 'Negócio:' : 'Bem-vindo,'}</p>
            <p className="font-semibold text-[#075E54]">{user?.businessName || user?.email}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
