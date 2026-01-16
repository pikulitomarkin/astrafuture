export interface Appointment {
  id: string
  tenantId: string
  customerId: string
  resourceId?: string
  startTime: string
  endTime: string
  status: 'scheduled' | 'confirmed' | 'completed' | 'cancelled' | 'no_show'
  notes?: string
  reminderSent: boolean
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
  tenantId: string
}

export interface AuthResponse {
  token: string
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
}
