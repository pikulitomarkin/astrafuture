export interface Appointment {
  id: string
  tenantId: string
  customerId: string
  resourceId?: string
  title?: string
  description?: string
  scheduledAt: string  // Campo retornado pelo backend
  endsAt: string        // Campo retornado pelo backend
  startTime?: string    // Manter para compatibilidade
  endTime?: string      // Manter para compatibilidade
  durationMinutes?: number
  status: 'scheduled' | 'confirmed' | 'completed' | 'cancelled' | 'no_show'
  appointmentType?: string
  notes?: string
  reminderSent?: boolean
  createdAt: string
  updatedAt: string
  customer?: Customer
  resource?: Resource
}

export interface Customer {
  id: string
  tenantId: string
  name: string
  phone: string
  email?: string
  isActive: boolean
  metadata?: Record<string, any>
  createdAt: string
  updatedAt: string
}

export interface Resource {
  id: string
  tenantId: string
  name: string
  type: string
  isActive: boolean
  metadata?: Record<string, any>
  createdAt: string
  updatedAt: string
}

export interface User {
  id: string
  email: string
  tenantId?: string
  businessName?: string
  logoUrl?: string
  primaryColor?: string
}

export interface TenantSettings {
  name: string
  logoUrl?: string
  primaryColor?: string
  timezone?: string
  locale?: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken?: string
  expiresIn: number
  user: User
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  businessName: string
  fullName?: string
  tenantId?: string
}

export interface ApiKey {
  id: string
  key: string
  name: string
  description?: string
  isActive: boolean
  lastUsedAt?: string
  expiresAt: string
  usageCount: number
  rateLimit?: number
  createdAt: string
}

export interface CreateApiKeyRequest {
  name: string
  description?: string
  expiresInDays?: number
  rateLimit?: number
}

export interface WebhookUrls {
  webhookUrl: string
  createCustomerUrl: string
  createAppointmentUrl: string
  checkCustomerUrl: string
  instructions: string
}
