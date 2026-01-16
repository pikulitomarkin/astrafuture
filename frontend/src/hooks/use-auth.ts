import { useMutation } from '@tanstack/react-query'
import { useRouter } from 'next/navigation'
import { apiClient } from '@/lib/api-client'
import { useAuthStore } from '@/store/auth-store'
import type { LoginRequest, RegisterRequest } from '@/types'
import { toast } from 'sonner'

export function useLogin() {
  const router = useRouter()
  const setAuth = useAuthStore((state) => state.setAuth)

  return useMutation({
    mutationFn: (data: LoginRequest) => apiClient.login(data),
    onSuccess: (response) => {
      setAuth(response.user, response.token)
      apiClient.setToken(response.token)
      toast.success('Login realizado com sucesso!')
      router.push('/dashboard')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao fazer login')
    },
  })
}

export function useRegister() {
  const router = useRouter()
  const setAuth = useAuthStore((state) => state.setAuth)

  return useMutation({
    mutationFn: (data: RegisterRequest) => apiClient.register(data),
    onSuccess: (response) => {
      setAuth(response.user, response.token)
      apiClient.setToken(response.token)
      toast.success('Conta criada com sucesso!')
      router.push('/dashboard')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Erro ao criar conta')
    },
  })
}

export function useLogout() {
  const router = useRouter()
  const clearAuth = useAuthStore((state) => state.clearAuth)

  return () => {
    clearAuth()
    toast.success('Logout realizado com sucesso!')
    router.push('/login')
  }
}

export function useAuth() {
  const { user, token, isAuthenticated } = useAuthStore()
  const logout = useLogout()

  return {
    user,
    token,
    isAuthenticated,
    logout,
  }
}
