'use client'

import { useState, useEffect } from 'react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { useCreateAppointment, useUpdateAppointment } from '@/hooks/use-appointments'
import { useCustomers } from '@/hooks/use-customers'
import { useResources } from '@/hooks/use-resources'
import type { Appointment } from '@/types'
import { format } from 'date-fns'

interface AppointmentDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  appointment?: Appointment
}

export function AppointmentDialog({ open, onOpenChange, appointment }: AppointmentDialogProps) {
  const { data: customers } = useCustomers()
  const { data: resources } = useResources()

  const [customerId, setCustomerId] = useState(appointment?.customerId || '')
  const [resourceId, setResourceId] = useState(appointment?.resourceId || '')
  const [startTime, setStartTime] = useState(
    appointment?.startTime ? format(new Date(appointment.startTime), "yyyy-MM-dd'T'HH:mm") : ''
  )
  const [endTime, setEndTime] = useState(
    appointment?.endTime ? format(new Date(appointment.endTime), "yyyy-MM-dd'T'HH:mm") : ''
  )
  const [status, setStatus] = useState(appointment?.status || 'scheduled')
  const [notes, setNotes] = useState(appointment?.notes || '')

  const createAppointment = useCreateAppointment()
  const updateAppointment = useUpdateAppointment()

  const isEditing = !!appointment

  useEffect(() => {
    if (appointment) {
      setCustomerId(appointment.customerId)
      setResourceId(appointment.resourceId || '')
      setStartTime(format(new Date(appointment.startTime), "yyyy-MM-dd'T'HH:mm"))
      setEndTime(format(new Date(appointment.endTime), "yyyy-MM-dd'T'HH:mm"))
      setStatus(appointment.status)
      setNotes(appointment.notes || '')
    }
  }, [appointment])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const data = {
      customerId,
      resourceId: resourceId || undefined,
      startTime: new Date(startTime).toISOString(),
      endTime: new Date(endTime).toISOString(),
      status,
      notes: notes || undefined,
    }

    if (isEditing) {
      await updateAppointment.mutateAsync({ id: appointment.id, data })
    } else {
      await createAppointment.mutateAsync(data)
    }

    onOpenChange(false)
    resetForm()
  }

  const resetForm = () => {
    setCustomerId('')
    setResourceId('')
    setStartTime('')
    setEndTime('')
    setStatus('scheduled')
    setNotes('')
  }

  const isLoading = createAppointment.isPending || updateAppointment.isPending

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[550px] max-h-[90vh] overflow-y-auto">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>
              {isEditing ? 'Editar Agendamento' : 'Novo Agendamento'}
            </DialogTitle>
            <DialogDescription>
              {isEditing
                ? 'Atualize as informações do agendamento'
                : 'Preencha os dados do novo agendamento'}
            </DialogDescription>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="customer">
                Cliente <span className="text-red-500">*</span>
              </Label>
              <select
                id="customer"
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                required
                disabled={isLoading}
                className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <option value="">Selecione um cliente</option>
                {customers?.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="resource">Recurso/Profissional</Label>
              <select
                id="resource"
                value={resourceId}
                onChange={(e) => setResourceId(e.target.value)}
                disabled={isLoading}
                className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <option value="">Nenhum recurso selecionado</option>
                {resources?.map((resource) => (
                  <option key={resource.id} value={resource.id}>
                    {resource.name} ({resource.type})
                  </option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="startTime">
                  Data/Hora Início <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="startTime"
                  type="datetime-local"
                  value={startTime}
                  onChange={(e) => setStartTime(e.target.value)}
                  required
                  disabled={isLoading}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="endTime">
                  Data/Hora Fim <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="endTime"
                  type="datetime-local"
                  value={endTime}
                  onChange={(e) => setEndTime(e.target.value)}
                  required
                  disabled={isLoading}
                />
              </div>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="status">
                Status <span className="text-red-500">*</span>
              </Label>
              <select
                id="status"
                value={status}
                onChange={(e) => setStatus(e.target.value as any)}
                required
                disabled={isLoading}
                className="flex h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm ring-offset-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gray-950 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <option value="scheduled">Agendado</option>
                <option value="confirmed">Confirmado</option>
                <option value="completed">Concluído</option>
                <option value="cancelled">Cancelado</option>
                <option value="no_show">Não compareceu</option>
              </select>
            </div>

            <div className="grid gap-2">
              <Label htmlFor="notes">Observações</Label>
              <Textarea
                id="notes"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                placeholder="Adicione observações sobre o agendamento..."
                disabled={isLoading}
                rows={3}
              />
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                onOpenChange(false)
                resetForm()
              }}
              disabled={isLoading}
            >
              Cancelar
            </Button>
            <Button 
              type="submit" 
              disabled={isLoading || !customerId || !startTime || !endTime}
            >
              {isLoading ? 'Salvando...' : isEditing ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
