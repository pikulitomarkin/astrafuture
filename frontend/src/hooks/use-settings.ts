import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import type { TenantSettings } from '@/types'
import { toast } from 'sonner'

export function useSettings() {
  return useQuery({
    queryKey: ['settings'],
    queryFn: () => apiClient.getSettings(),
  })
}

export function useUpdateSettings() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: Partial<TenantSettings>) => apiClient.updateSettings(data),
    onMutate: async (data) => {
      // Cancela queries em andamento
      await queryClient.cancelQueries({ queryKey: ['settings'] })

      // Snapshot do estado anterior
      const previousSettings = queryClient.getQueryData<TenantSettings>(['settings'])

      // Atualiza otimisticamente
      if (previousSettings) {
        queryClient.setQueryData<TenantSettings>(['settings'], {
          ...previousSettings,
          ...data
        })
      }

      return { previousSettings }
    },
    onSuccess: () => {
      toast.success('Configurações atualizadas com sucesso!')
    },
    onError: (error: any, variables, context) => {
      // Rollback
      if (context?.previousSettings) {
        queryClient.setQueryData(['settings'], context.previousSettings)
      }
      toast.error(error.response?.data?.message || 'Erro ao atualizar configurações')
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['settings'] })
    },
  })
}
