'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { Calendar, Home, Users, LogOut, Plug, Settings } from 'lucide-react'
import { cn } from '@/lib/utils'
import { useAuth } from '@/hooks/use-auth'
import { useSettings } from '@/hooks/use-settings'
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
  {
    title: 'Configurações',
    href: '/dashboard/settings',
    icon: Settings,
  },
]

export function Sidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuth()
  const { data: settings } = useSettings()

  const businessName = settings?.name || user?.businessName || 'Astra Agenda'
  const logoUrl = settings?.logoUrl

  return (
    <div className="flex flex-col h-full w-64 bg-white border-r border-gray-200 shadow-sm">
      {/* Header com Logo Customizado */}
      <div className="p-4 border-b border-gray-200 bg-white">
        <div className="flex flex-col space-y-2">
          {logoUrl ? (
            <div className="flex items-center justify-center">
              <img 
                src={logoUrl} 
                alt={businessName} 
                className="h-12 w-auto max-w-full object-contain"
                onError={(e) => {
                  // Fallback para logo padrão se a imagem falhar
                  e.currentTarget.style.display = 'none'
                  e.currentTarget.parentElement?.insertAdjacentHTML('beforeend', 
                    `<div class="text-[#075E54] font-bold text-xl">${businessName}</div>`)
                }}
              />
            </div>
          ) : (
            <div className="flex items-center justify-center">
              <img 
                src="/logo.svg" 
                alt="Astra Agenda" 
                className="w-full h-auto"
              />
            </div>
          )}
          <div className="text-center space-y-1">
            {logoUrl && (
              <p className="text-sm font-semibold text-[#075E54]">{businessName}</p>
            )}
            {user && (
              <p className="text-xs text-[#333333]">{user.email}</p>
            )}
          </div>
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
