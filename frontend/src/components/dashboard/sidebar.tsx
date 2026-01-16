'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { Calendar, Home, Users, LogOut } from 'lucide-react'
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
]

export function Sidebar() {
  const pathname = usePathname()
  const { user, logout } = useAuth()

  return (
    <div className="flex flex-col h-full w-64 bg-white border-r">
      <div className="p-6 border-b">
        <h1 className="text-2xl font-bold text-primary">AstraFuture</h1>
        {user && (
          <p className="text-sm text-muted-foreground mt-1">{user.email}</p>
        )}
      </div>
      
      <nav className="flex-1 p-4 space-y-2">
        {menuItems.map((item) => {
          const Icon = item.icon
          const isActive = pathname === item.href
          
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                'flex items-center space-x-3 px-4 py-3 rounded-lg transition-colors',
                isActive
                  ? 'bg-primary text-white'
                  : 'text-gray-700 hover:bg-gray-100'
              )}
            >
              <Icon className="h-5 w-5" />
              <span>{item.title}</span>
            </Link>
          )
        })}
      </nav>

      <div className="p-4 border-t">
        <Button
          variant="ghost"
          className="w-full justify-start"
          onClick={logout}
        >
          <LogOut className="h-5 w-5 mr-3" />
          Sair
        </Button>
      </div>
    </div>
  )
}
