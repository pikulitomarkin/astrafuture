import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Calendar, Clock, User, Trash2, Edit } from 'lucide-react'
import type { Appointment } from '@/types'
import { formatDate, formatTime, cn } from '@/lib/utils'

interface AppointmentCardProps {
  appointment: Appointment
  onEdit?: (appointment: Appointment) => void
  onDelete?: (id: string) => void
}

const statusColors = {
  scheduled: 'bg-blue-100 text-blue-800',
  confirmed: 'bg-green-100 text-green-800',
  completed: 'bg-gray-100 text-gray-800',
  cancelled: 'bg-red-100 text-red-800',
  no_show: 'bg-orange-100 text-orange-800',
}

const statusLabels = {
  scheduled: 'Agendado',
  confirmed: 'Confirmado',
  completed: 'Completo',
  cancelled: 'Cancelado',
  no_show: 'Não compareceu',
}

export function AppointmentCard({ appointment, onEdit, onDelete }: AppointmentCardProps) {
  return (
    <Card className="hover:shadow-md transition-shadow">
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <div className="flex-1 space-y-3">
            <div className="flex items-center justify-between">
              <span
                className={cn(
                  'px-3 py-1 rounded-full text-xs font-medium',
                  statusColors[appointment.status]
                )}
              >
                {statusLabels[appointment.status]}
              </span>
            </div>

            <div className="space-y-2">
              <div className="flex items-center text-gray-700">
                <Calendar className="h-4 w-4 mr-2 text-gray-400" />
                <span className="font-medium">{formatDate(appointment.startTime)}</span>
              </div>

              <div className="flex items-center text-gray-700">
                <Clock className="h-4 w-4 mr-2 text-gray-400" />
                <span>
                  {formatTime(appointment.startTime)} - {formatTime(appointment.endTime)}
                </span>
              </div>

              {appointment.customer && (
                <div className="flex items-center text-gray-700">
                  <User className="h-4 w-4 mr-2 text-gray-400" />
                  <span>{appointment.customer.name}</span>
                </div>
              )}

              {appointment.notes && (
                <div className="mt-3 p-3 bg-gray-50 rounded text-sm text-gray-600">
                  {appointment.notes}
                </div>
              )}
            </div>
          </div>

          <div className="flex flex-col space-y-2 ml-4">
            {onEdit && (
              <Button
                variant="ghost"
                size="icon"
                onClick={() => onEdit(appointment)}
              >
                <Edit className="h-4 w-4" />
              </Button>
            )}
            {onDelete && (
              <Button
                variant="ghost"
                size="icon"
                onClick={() => onDelete(appointment.id)}
              >
                <Trash2 className="h-4 w-4 text-red-500" />
              </Button>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
