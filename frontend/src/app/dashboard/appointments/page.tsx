'use client'

import { useState } from 'react'
import { Header } from '@/components/dashboard/header'
import { AppointmentCard } from '@/components/appointments/appointment-card'
import { Button } from '@/components/ui/button'
import { Plus } from 'lucide-react'
import { useAppointments, useDeleteAppointment } from '@/hooks/use-appointments'

export default function AppointmentsPage() {
  const { data: appointments, isLoading, error } = useAppointments()
  const deleteAppointment = useDeleteAppointment()

  const handleDelete = (id: string) => {
    if (confirm('Tem certeza que deseja excluir este agendamento?')) {
      deleteAppointment.mutate(id)
    }
  }

  return (
    <div>
      <Header
        title="Agendamentos"
        description="Gerencie todos os seus agendamentos"
      />

      <div className="p-8">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-2xl font-bold text-gray-900">
              Todos os Agendamentos
            </h2>
            <p className="text-gray-600 mt-1">
              {appointments?.length || 0} agendamento(s) encontrado(s)
            </p>
          </div>
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Novo Agendamento
          </Button>
        </div>

        {isLoading && (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-red-800">
            Erro ao carregar agendamentos. Por favor, tente novamente.
          </div>
        )}

        {!isLoading && !error && appointments && appointments.length === 0 && (
          <div className="text-center py-12">
            <div className="text-gray-400 mb-4">
              <svg
                className="mx-auto h-12 w-12"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">
              Nenhum agendamento encontrado
            </h3>
            <p className="text-gray-600 mb-6">
              Comece criando seu primeiro agendamento
            </p>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Criar Primeiro Agendamento
            </Button>
          </div>
        )}

        {!isLoading && !error && appointments && appointments.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {appointments.map((appointment) => (
              <AppointmentCard
                key={appointment.id}
                appointment={appointment}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
