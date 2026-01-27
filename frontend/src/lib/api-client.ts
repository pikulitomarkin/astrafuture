import axios, { type AxiosInstance } from 'axios'
import type { 
  Appointment, 
  AuthResponse, 
  LoginRequest, 
  RegisterRequest,
  Customer,
  Resource,
  ApiKey,
  CreateApiKeyRequest,
  WebhookUrls
} from '@/types'

class ApiClient {
  private client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    // Interceptor para adicionar token
    this.client.interceptors.request.use((config) => {
      const token = this.getToken()
      if (token) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    })

    // Interceptor para tratar erros de autenticação
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          this.clearToken()
          if (typeof window !== 'undefined') {
            window.location.href = '/login'
          }
        }
        return Promise.reject(error)
      }
    )
  }

  private getToken(): string | null {
    if (typeof window === 'undefined') return null
    return localStorage.getItem('token')
  }

  private clearToken(): void {
    if (typeof window === 'undefined') return
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  setToken(token: string): void {
    if (typeof window === 'undefined') return
    localStorage.setItem('token', token)
  }

  // Auth
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/auth/login', data)
    return response.data
  }

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await this.client.post<AuthResponse>('/auth/register', data)
    return response.data
  }

  // Appointments
  async getAppointments(): Promise<Appointment[]> {
    const response = await this.client.get<Appointment[]>('/appointments')
    return response.data
  }

  async getAppointment(id: string): Promise<Appointment> {
    const response = await this.client.get<Appointment>(`/appointments/${id}`)
    return response.data
  }

  async createAppointment(data: Partial<Appointment>): Promise<Appointment> {
    const response = await this.client.post<Appointment>('/appointments', data)
    return response.data
  }

  async updateAppointment(id: string, data: Partial<Appointment>): Promise<Appointment> {
    const response = await this.client.put<Appointment>(`/appointments/${id}`, data)
    return response.data
  }

  async deleteAppointment(id: string): Promise<void> {
    await this.client.delete(`/appointments/${id}`)
  }

  // Customers
  async getCustomers(): Promise<Customer[]> {
    const response = await this.client.get<Customer[]>('/customers')
    return response.data
  }

  async getCustomer(id: string): Promise<Customer> {
    const response = await this.client.get<Customer>(`/customers/${id}`)
    return response.data
  }

  async createCustomer(data: Partial<Customer>): Promise<Customer> {
    const response = await this.client.post<Customer>('/customers', data)
    return response.data
  }

  async updateCustomer(id: string, data: Partial<Customer>): Promise<Customer> {
    const response = await this.client.put<Customer>(`/customers/${id}`, data)
    return response.data
  }

  async deleteCustomer(id: string): Promise<void> {
    await this.client.delete(`/customers/${id}`)
  }

  // Resources
  async getResources(): Promise<Resource[]> {
    const response = await this.client.get<Resource[]>('/resources')
    return response.data
  }

  async getResource(id: string): Promise<Resource> {
    const response = await this.client.get<Resource>(`/resources/${id}`)
    return response.data
  }

  // API Keys
  async getApiKeys(): Promise<ApiKey[]> {
    const response = await this.client.get<ApiKey[]>('/apikeys')
    return response.data
  }

  async createApiKey(data: CreateApiKeyRequest): Promise<ApiKey> {
    const response = await this.client.post<ApiKey>('/apikeys', data)
    return response.data
  }

  async updateApiKey(id: string, data: { name: string; description?: string; isActive: boolean }): Promise<void> {
    await this.client.put(`/apikeys/${id}`, data)
  }

  async deleteApiKey(id: string): Promise<void> {
    await this.client.delete(`/apikeys/${id}`)
  }

  async getWebhookUrls(): Promise<WebhookUrls> {
    const response = await this.client.get<WebhookUrls>('/apikeys/webhook-url')
    return response.data
  }
}

export const apiClient = new ApiClient()
