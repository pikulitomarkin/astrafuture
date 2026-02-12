import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Appointment } from '@/types'
import { toast } from 'sonner'

export function useAppointments() {
  return useQuery({
    queryKey: ['appointments'],
    queryFn: () => apiClient.getAppointments(),
  })
}

export function useAppointment(id: string) {
  return useQuery({
    queryKey: ['appointments', id],
    queryFn: () => apiClient.getAppointment(id),
    enabled: !!id,
  })
}

export function useCreateAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Partial<Appointment>) => apiClient.createAppointment(data),
    onMutate: async () => {
      // Cancela queries em andamento para evitar sobrescrever updates otimistas
      await queryClient.cancelQueries({ queryKey: ['appointments'] })
    },
    onSuccess: () => {
      toast.success('Agendamento criado com sucesso!')
      // Refetch imediato
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar agendamento')
    },
  })
}

export function useUpdateAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Appointment> }) =>
      apiClient.updateAppointment(id, data),
    onMutate: async ({ id, data }) => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['appointments'] })

      // Snapshot do estado anterior
      const previousAppointments = queryClient.getQueryData<Appointment[]>(['appointments'])

      // Atualiza otimisticamente
      if (previousAppointments) {
        queryClient.setQueryData<Appointment[]>(
          ['appointments'],
          previousAppointments.map(apt => apt.id === id ? { ...apt, ...data } : apt)
        )
      }

      return { previousAppointments }
    },
    onSuccess: () => {
      toast.success('Agendamento atualizado com sucesso!')
    },
    onError: (error: any, variables, context) => {
      // Rollback
      if (context?.previousAppointments) {
        queryClient.setQueryData(['appointments'], context.previousAppointments)
      }
      toast.error(error.response?.data?.message || 'Erro ao atualizar agendamento')
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
  })
}

export function useDeleteAppointment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => apiClient.deleteAppointment(id),
    onMutate: async (id: string) => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['appointments'] })

      // Snapshot do estado anterior
      const previousAppointments = queryClient.getQueryData<Appointment[]>(['appointments'])

      // Remove otimisticamente da UI
      if (previousAppointments) {
        queryClient.setQueryData<Appointment[]>(
          ['appointments'],
          previousAppointments.filter(apt => apt.id !== id)
        )
      }

      // Retorna contexto para rollback se falhar
      return { previousAppointments }
    },
    onSuccess: () => {
      toast.success('Agendamento excluído com sucesso!')
    },
    onError: (error: any, id, context) => {
      // Rollback em caso de erro
      if (context?.previousAppointments) {
        queryClient.setQueryData(['appointments'], context.previousAppointments)
      }
      toast.error(error.response?.data?.message || 'Erro ao excluir agendamento')
    },
    onSettled: () => {
      // Refetch para garantir sincronização
      queryClient.invalidateQueries({ queryKey: ['appointments'] })
    },
  })
}
