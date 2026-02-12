import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { Customer } from '@/types'
import { toast } from 'sonner'

export function useCustomers() {
  return useQuery({
    queryKey: ['customers'],
    queryFn: () => apiClient.getCustomers(),
  })
}

export function useCustomer(id: string) {
  return useQuery({
    queryKey: ['customers', id],
    queryFn: () => apiClient.getCustomer(id),
    enabled: !!id,
  })
}

export function useCreateCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Partial<Customer>) => apiClient.createCustomer(data),
    onMutate: async () => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['customers'] })
    },
    onSuccess: () => {
      toast.success('Cliente criado com sucesso!')
      // Refetch imediato
      queryClient.invalidateQueries({ queryKey: ['customers'] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar cliente')
    },
  })
}

export function useUpdateCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Customer> }) =>
      apiClient.updateCustomer(id, data),
    onMutate: async ({ id, data }) => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['customers'] })

      // Snapshot do estado anterior
      const previousCustomers = queryClient.getQueryData<Customer[]>(['customers'])

      // Atualiza otimisticamente
      if (previousCustomers) {
        queryClient.setQueryData<Customer[]>(
          ['customers'],
          previousCustomers.map(customer => customer.id === id ? { ...customer, ...data } : customer)
        )
      }

      return { previousCustomers }
    },
    onSuccess: () => {
      toast.success('Cliente atualizado com sucesso!')
    },
    onError: (error: any, variables, context) => {
      // Rollback
      if (context?.previousCustomers) {
        queryClient.setQueryData(['customers'], context.previousCustomers)
      }
      toast.error(error.response?.data?.message || 'Erro ao atualizar cliente')
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] })
    },
  })
}

export function useDeleteCustomer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => apiClient.deleteCustomer(id),
    onMutate: async (id: string) => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['customers'] })

      // Snapshot do estado anterior
      const previousCustomers = queryClient.getQueryData<Customer[]>(['customers'])

      // Remove otimisticamente da UI
      if (previousCustomers) {
        queryClient.setQueryData<Customer[]>(
          ['customers'],
          previousCustomers.filter(customer => customer.id !== id)
        )
      }

      // Retorna contexto para rollback se falhar
      return { previousCustomers }
    },
    onSuccess: () => {
      toast.success('Cliente excluído com sucesso!')
    },
    onError: (error: any, id, context) => {
      // Rollback em caso de erro
      if (context?.previousCustomers) {
        queryClient.setQueryData(['customers'], context.previousCustomers)
      }
      toast.error(error.response?.data?.message || 'Erro ao excluir cliente')
    },
    onSettled: () => {
      // Refetch para garantir sincronização
      queryClient.invalidateQueries({ queryKey: ['customers'] })
    },
  })
}
