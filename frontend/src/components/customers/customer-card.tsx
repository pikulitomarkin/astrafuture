'use client'

import { Mail, Phone, MoreVertical, Trash2, Edit } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { Customer } from '@/types'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'

interface CustomerCardProps {
  customer: Customer
  onEdit: (customer: Customer) => void
  onDelete: (id: string) => void
}

export function CustomerCard({ customer, onEdit, onDelete }: CustomerCardProps) {
  return (
    <div className="bg-white rounded-lg border border-gray-200 p-6 hover:shadow-md transition-shadow">
      <div className="flex items-start justify-between mb-4">
        <div className="flex-1">
          <h3 className="text-lg font-semibold text-gray-900 mb-1">
            {customer.name}
          </h3>
          <div className="flex items-center gap-4 text-sm text-gray-600">
            {customer.phone && (
              <div className="flex items-center gap-1">
                <Phone className="h-4 w-4" />
                <span>{customer.phone}</span>
              </div>
            )}
            {customer.email && (
              <div className="flex items-center gap-1">
                <Mail className="h-4 w-4" />
                <span>{customer.email}</span>
              </div>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2">
          {!customer.isActive && (
            <span className="px-2 py-1 text-xs font-medium bg-gray-100 text-gray-600 rounded">
              Inativo
            </span>
          )}
        </div>
      </div>

      <div className="text-xs text-gray-500 mb-4">
        Cliente desde {format(new Date(customer.createdAt), 'dd/MM/yyyy', { locale: ptBR })}
      </div>

      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          className="flex-1"
          onClick={() => onEdit(customer)}
        >
          <Edit className="h-4 w-4 mr-2" />
          Editar
        </Button>
        <Button
          variant="outline"
          size="sm"
          className="text-red-600 hover:text-red-700 hover:bg-red-50"
          onClick={() => onDelete(customer.id)}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    </div>
  )
}
