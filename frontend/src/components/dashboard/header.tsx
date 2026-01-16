'use client'

import { useAuth } from '@/hooks/use-auth'

interface HeaderProps {
  title: string
  description?: string
}

export function Header({ title, description }: HeaderProps) {
  const { user } = useAuth()

  return (
    <div className="border-b bg-white px-8 py-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{title}</h1>
          {description && (
            <p className="text-gray-600 mt-1">{description}</p>
          )}
        </div>
        <div className="flex items-center space-x-4">
          <div className="text-right">
            <p className="text-sm text-gray-600">Bem-vindo,</p>
            <p className="font-medium text-gray-900">{user?.email}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
