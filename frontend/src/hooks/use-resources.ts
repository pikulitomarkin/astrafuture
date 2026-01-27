import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'

export function useResources() {
  return useQuery({
    queryKey: ['resources'],
    queryFn: () => apiClient.getResources(),
  })
}

export function useResource(id: string) {
  return useQuery({
    queryKey: ['resources', id],
    queryFn: () => apiClient.getResource(id),
    enabled: !!id,
  })
}
