import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { User } from '@/types'

interface AuthState {
  user: User | null
  token: string | null
  setAuth: (user: User, token: string) => void
  clearAuth: () => void
  isAuthenticated: boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,
      setAuth: (user, token) => {
        set({ user, token, isAuthenticated: true })
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', token)
          localStorage.setItem('user', JSON.stringify(user))
        }
      },
      clearAuth: () => {
        set({ user: null, token: null, isAuthenticated: false })
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token')
          localStorage.removeItem('user')
        }
      },
    }),
    {
      name: 'auth-storage',
    }
  )
)
