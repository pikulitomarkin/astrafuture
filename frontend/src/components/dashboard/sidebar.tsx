'use client'

import Link from 'next/link'
import Image from 'next/image'
import { usePathname } from 'next/navigation'
import { Calendar, Home, Users, LogOut, Plug } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/use-auth'
import { Button } from '@/components/ui/button'

const menuItems = [
  {
    title: 'Dashboard',
    href: '/dashboard',
    icon: Home,
  },
  {
    title: 'Agendamentos',
    href: '/dashboard/appointments',
    icon: Calendar,
  },
  {
    title: 'Clientes',
    href: '/dashboard/customers',
    icon: Users,
  },
  {
    title: 'Integrações',
    href: '/dashboard/integrations',
    icon: Plug,
  },
]

export function Sidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuth()

  return (
    <div className="flex flex-col h-full w-64 bg-white border-r border-gray-200 shadow-sm">
      {/* Header com Logo */}
      <div className="p-4 border-b border-gray-200 bg-white">
        <div className="flex flex-col space-y-2">
          <Image 
            src="/logo.svg" 
            alt="Astra Agenda" 
            width={180} 
            height={60}
            className="w-full h-auto"
            priority
          />
          {user && (
            <p className="text-xs text-[#333333] text-center">{user.email}</p>
          )}
        </div>
      </div>
      
      {/* Menu de Navegação */}
      <nav className="flex-1 p-4 space-y-2">
        {menuItems.map((item) => {
          const Icon = item.icon
          const isActive = pathname === item.href
          
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                'flex items-center space-x-3 px-4 py-3 rounded-lg transition-all font-medium',
                isActive
                  ? 'bg-[#25D366] text-white shadow-md'
                  : 'text-[#333333] hover:bg-gray-100'
              )}
            >
              <Icon className="h-5 w-5" />
              <span>{item.title}</span>
            </Link>
          )
        })}
      </nav>

      {/* Botão Sair */}
      <div className="p-4 border-t border-gray-200">
        <Button
          variant="ghost"
          className="w-full justify-start text-[#333333] hover:bg-gray-100 hover:text-[#075E54]"
          onClick={logout}
        >
          <LogOut className="h-5 w-5 mr-3" />
          Sair
        </Button>
      </div>
    </div>
  )
}
