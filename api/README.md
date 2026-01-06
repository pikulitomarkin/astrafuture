# API Specification - AstraFuture

## Design Principles

1. **RESTful**: Recursos como substantivos, verbos HTTP para ações
2. **Stateless**: Cada request contém toda informação necessária (JWT)
3. **Versionamento**: `/api/v1/` para evolução sem breaking changes
4. **Hypermedia**: HATEOAS para navegabilidade (links em responses)
5. **Rate Limiting**: Proteger contra abuso

---

## Base URL

```
Development:  http://localhost:5000/api/v1
Staging:      https://api-staging.astrafuture.app/api/v1
Production:   https://api.astrafuture.app/api/v1
```

---

## Authentication

### JWT Bearer Token

**Headers:**
```http
Authorization: Bearer <JWT_TOKEN>
```

**JWT Claims:**
```json
{
  "sub": "user-uuid",
  "tenant_id": "tenant-uuid",
  "role": "admin",
  "permissions": ["appointments:write", "customers:read"],
  "exp": 1640995200
}
```

### Public Endpoints (sem autenticação)
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/magic-link`
- `GET /health`

---

## Error Handling

### Standard Error Response
```json
{
  "error": {
    "code": "APPOINTMENT_CONFLICT",
    "message": "Já existe um agendamento neste horário",
    "details": {
      "resource_id": "abc-123",
      "conflicting_appointment_id": "xyz-789",
      "suggested_slots": [
        "2026-01-10T15:00:00Z",
        "2026-01-10T16:00:00Z"
      ]
    },
    "timestamp": "2026-01-05T10:30:00Z",
    "request_id": "req-uuid-123"
  }
}
```

### HTTP Status Codes
| Code | Significado | Uso |
|------|-------------|-----|
| 200 | OK | Sucesso em GET, PUT, PATCH |
| 201 | Created | Sucesso em POST (criar recurso) |
| 204 | No Content | Sucesso em DELETE |
| 400 | Bad Request | Validação falhou |
| 401 | Unauthorized | Token inválido/ausente |
| 403 | Forbidden | Sem permissão para ação |
| 404 | Not Found | Recurso não existe |
| 409 | Conflict | Conflito (ex: appointment duplicado) |
| 422 | Unprocessable Entity | Business rule violation |
| 429 | Too Many Requests | Rate limit excedido |
| 500 | Internal Server Error | Erro inesperado |

---

## Rate Limiting

### Headers de Resposta
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 87
X-RateLimit-Reset: 1640995200
```

### Limites por Tier
| Tier | Requests/min | Requests/dia |
|------|--------------|--------------|
| Free | 100 | 10,000 |
| Pro | 1,000 | 100,000 |
| Enterprise | Custom | Custom |

---

## Endpoints

### 1. Authentication

#### POST `/auth/register`
**Descrição:** Criar nova conta via WhatsApp (onboarding rápido).

**Request:**
```json
{
  "phone": "+5511987654321",
  "full_name": "Dr. João Silva",
  "tenant_name": "Clínica Psique",
  "tenant_type": "psychology"
}
```

**Response (201):**
```json
{
  "data": {
    "user_id": "user-uuid",
    "tenant_id": "tenant-uuid",
    "magic_link": "https://app.astrafuture.app/auth?token=xyz",
    "expires_in": 600
  }
}
```

---

#### POST `/auth/magic-link`
**Descrição:** Solicitar link mágico por WhatsApp/Email.

**Request:**
```json
{
  "phone": "+5511987654321",
  "channel": "whatsapp"
}
```

**Response (200):**
```json
{
  "data": {
    "message": "Link enviado via WhatsApp",
    "expires_in": 600
  }
}
```

---

### 2. Tenants

#### GET `/tenants/me`
**Descrição:** Obter informações do tenant atual.

**Response (200):**
```json
{
  "data": {
    "id": "tenant-uuid",
    "name": "Clínica Psique",
    "slug": "clinica-psique",
    "tenant_type": "psychology",
    "timezone": "America/Sao_Paulo",
    "subscription_tier": "pro",
    "logo_url": "https://cdn.pilar1.app/logos/abc.png",
    "primary_color": "#3B82F6",
    "business_rules": {
      "min_duration_minutes": 50,
      "allow_online_sessions": true
    },
    "created_at": "2025-12-01T10:00:00Z"
  }
}
```

---

#### PATCH `/tenants/me`
**Descrição:** Atualizar configurações do tenant.

**Request:**
```json
{
  "primary_color": "#10B981",
  "business_rules": {
    "cancellation_policy_hours": 48
  }
}
```

**Response (200):** Tenant atualizado

---

### 3. Users

#### GET `/users`
**Descrição:** Listar usuários do tenant.

**Query Params:**
```
?role=admin&is_active=true&page=1&limit=20
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "user-uuid",
      "full_name": "Dra. Maria Santos",
      "email": "maria@clinica.com",
      "role": "admin",
      "professional_title": "Psicóloga",
      "avatar_url": "https://cdn.pilar1.app/avatars/xyz.jpg",
      "is_active": true,
      "created_at": "2025-12-01T10:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 45,
    "total_pages": 3
  }
}
```

---

#### POST `/users`
**Descrição:** Convidar novo usuário.

**Request:**
```json
{
  "email": "novo@clinica.com",
  "full_name": "Dr. Pedro Oliveira",
  "role": "member",
  "permissions": ["appointments:read", "appointments:write"]
}
```

**Response (201):** Usuário criado + email de convite enviado

---

### 4. Resources

#### GET `/resources`
**Descrição:** Listar recursos agendáveis.

**Query Params:**
```
?resource_type=professional&is_active=true
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "resource-uuid",
      "name": "Dra. Maria Santos",
      "resource_type": "professional",
      "working_hours": {
        "monday": [{"start": "09:00", "end": "18:00"}]
      },
      "booking_buffer_minutes": 10,
      "is_active": true
    }
  ]
}
```

---

#### POST `/resources`
**Descrição:** Criar novo resource.

**Request:**
```json
{
  "name": "Sala 101",
  "resource_type": "room",
  "location": "1º Andar",
  "working_hours": {
    "monday": [{"start": "08:00", "end": "20:00"}],
    "tuesday": [{"start": "08:00", "end": "20:00"}]
  },
  "max_simultaneous_bookings": 1
}
```

**Response (201):** Resource criado

---

### 5. Customers

#### GET `/customers`
**Descrição:** Listar clientes do tenant.

**Query Params:**
```
?search=maria&tags=vip&page=1&limit=20
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "customer-uuid",
      "full_name": "Maria da Silva",
      "email": "maria@email.com",
      "phone": "+5511987654321",
      "tags": ["vip", "ansiedade"],
      "total_appointments": 12,
      "last_appointment_at": "2025-12-15T14:00:00Z",
      "created_at": "2025-01-10T10:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "limit": 20,
    "total": 234
  }
}
```

---

#### POST `/customers`
**Descrição:** Criar novo cliente.

**Request:**
```json
{
  "full_name": "João Pedro Costa",
  "phone": "+5511999887766",
  "email": "joao@email.com",
  "birth_date": "1990-05-15",
  "lead_source": "whatsapp",
  "tags": ["primeira-consulta"],
  "meta_data": {
    "emergency_contact": {
      "name": "Ana Costa",
      "phone": "+5511888777666"
    }
  }
}
```

**Response (201):** Cliente criado

---

#### GET `/customers/:id`
**Descrição:** Detalhes completos do cliente.

**Response (200):**
```json
{
  "data": {
    "id": "customer-uuid",
    "full_name": "João Pedro Costa",
    "phone": "+5511999887766",
    "email": "joao@email.com",
    "tags": ["primeira-consulta", "ansiedade"],
    "meta_data": {
      "emergency_contact": {
        "name": "Ana Costa",
        "phone": "+5511888777666"
      },
      "anamnesis": "Texto longo da anamnese..."
    },
    "appointments_summary": {
      "total": 5,
      "completed": 4,
      "cancelled": 1,
      "next_appointment": {
        "id": "appt-uuid",
        "scheduled_at": "2026-01-15T10:00:00Z"
      }
    },
    "created_at": "2025-01-10T10:00:00Z"
  }
}
```

---

### 6. Appointments ⭐ (Core)

#### GET `/appointments`
**Descrição:** Listar appointments com filtros avançados.

**Query Params:**
```
?from=2026-01-01
&to=2026-01-31
&status=scheduled,confirmed
&resource_id=resource-uuid
&customer_id=customer-uuid
&page=1
&limit=50
&sort=scheduled_at:desc
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "appointment-uuid",
      "customer": {
        "id": "customer-uuid",
        "full_name": "Maria da Silva",
        "phone": "+5511987654321"
      },
      "resource": {
        "id": "resource-uuid",
        "name": "Dra. Maria Santos",
        "resource_type": "professional"
      },
      "scheduled_at": "2026-01-10T14:00:00Z",
      "duration_minutes": 60,
      "ends_at": "2026-01-10T15:00:00Z",
      "status": "confirmed",
      "title": "Consulta de Retorno",
      "appointment_type": "follow-up",
      "location": "Sala 101",
      "price_cents": 15000,
      "payment_status": "paid",
      "created_at": "2026-01-05T10:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "limit": 50,
    "total": 342
  }
}
```

---

#### POST `/appointments`
**Descrição:** Criar novo agendamento (com validação de conflitos).

**Request:**
```json
{
  "customer_id": "customer-uuid",
  "resource_id": "resource-uuid",
  "scheduled_at": "2026-01-10T14:00:00Z",
  "duration_minutes": 60,
  "title": "Primeira Consulta",
  "appointment_type": "initial",
  "location": "Sala 101",
  "price_cents": 15000,
  "meta_data": {
    "session_type": "presencial",
    "complaint": "Ansiedade"
  }
}
```

**Response (201):**
```json
{
  "data": {
    "id": "appointment-uuid",
    "customer_id": "customer-uuid",
    "resource_id": "resource-uuid",
    "scheduled_at": "2026-01-10T14:00:00Z",
    "duration_minutes": 60,
    "status": "scheduled",
    "confirmation_link": "https://app.pilar1.app/appointments/confirm/abc123"
  }
}
```

**Error (409 Conflict):**
```json
{
  "error": {
    "code": "APPOINTMENT_CONFLICT",
    "message": "Horário indisponível",
    "details": {
      "conflicting_appointment_id": "xyz-789",
      "suggested_slots": [
        "2026-01-10T15:00:00Z",
        "2026-01-10T16:00:00Z"
      ]
    }
  }
}
```

---

#### PATCH `/appointments/:id`
**Descrição:** Atualizar appointment (reagendar, mudar status).

**Request:**
```json
{
  "scheduled_at": "2026-01-10T15:00:00Z",
  "status": "confirmed"
}
```

**Response (200):** Appointment atualizado

---

#### DELETE `/appointments/:id`
**Descrição:** Cancelar appointment (soft delete).

**Request:**
```json
{
  "cancellation_reason": "Cliente solicitou cancelamento",
  "send_notification": true
}
```

**Response (204):** No Content

---

#### POST `/appointments/:id/reschedule`
**Descrição:** Reagendar com validação automática.

**Request:**
```json
{
  "new_scheduled_at": "2026-01-12T10:00:00Z",
  "reason": "Conflito de agenda do cliente",
  "notify_customer": true
}
```

**Response (200):** Appointment reagendado

---

#### POST `/appointments/:id/complete`
**Descrição:** Marcar como concluído + adicionar notas.

**Request:**
```json
{
  "meeting_notes": "Cliente evoluiu bem. Próxima sessão em 15 dias.",
  "meta_data": {
    "mood_rating": 4,
    "homework_assigned": "Diário de pensamentos"
  }
}
```

**Response (200):** Status atualizado para `completed`

---

### 7. Availability

#### GET `/availability/slots`
**Descrição:** Obter horários disponíveis para agendamento.

**Query Params:**
```
?resource_id=resource-uuid
&date=2026-01-10
&duration_minutes=60
```

**Response (200):**
```json
{
  "data": [
    {
      "slot_time": "2026-01-10T09:00:00Z",
      "is_available": true,
      "duration_minutes": 60
    },
    {
      "slot_time": "2026-01-10T10:00:00Z",
      "is_available": true,
      "duration_minutes": 60
    },
    {
      "slot_time": "2026-01-10T11:00:00Z",
      "is_available": false,
      "reason": "Já agendado"
    },
    {
      "slot_time": "2026-01-10T14:00:00Z",
      "is_available": true,
      "duration_minutes": 60
    }
  ],
  "meta": {
    "date": "2026-01-10",
    "resource_id": "resource-uuid",
    "total_slots": 16,
    "available_slots": 12
  }
}
```

---

#### POST `/availability/exceptions`
**Descrição:** Criar exceção de disponibilidade (férias, feriado).

**Request:**
```json
{
  "resource_id": "resource-uuid",
  "start_date": "2026-02-01",
  "end_date": "2026-02-14",
  "exception_type": "unavailable",
  "reason": "Férias",
  "all_day": true
}
```

**Response (201):** Exceção criada

---

### 8. Notifications

#### GET `/notifications`
**Descrição:** Listar notificações do usuário.

**Query Params:**
```
?status=pending&channel=in_app&page=1&limit=20
```

**Response (200):**
```json
{
  "data": [
    {
      "id": "notification-uuid",
      "type": "appointment_reminder",
      "channel": "in_app",
      "subject": "Lembrete de Consulta",
      "body": "Sua consulta é amanhã às 14h",
      "related_appointment_id": "appt-uuid",
      "status": "delivered",
      "created_at": "2026-01-09T10:00:00Z"
    }
  ]
}
```

---

#### PATCH `/notifications/:id/mark-read`
**Descrição:** Marcar notificação como lida.

**Response (200):** Notificação atualizada

---

### 9. Analytics

#### GET `/analytics/appointments`
**Descrição:** Métricas de appointments.

**Query Params:**
```
?from=2026-01-01&to=2026-01-31&group_by=day
```

**Response (200):**
```json
{
  "data": {
    "total_appointments": 234,
    "by_status": {
      "scheduled": 45,
      "confirmed": 78,
      "completed": 98,
      "cancelled": 13
    },
    "by_day": [
      {"date": "2026-01-01", "count": 12},
      {"date": "2026-01-02", "count": 15}
    ],
    "completion_rate": 0.87,
    "cancellation_rate": 0.05,
    "average_duration_minutes": 58
  }
}
```

---

#### GET `/analytics/revenue`
**Descrição:** Métricas de receita.

**Query Params:**
```
?from=2026-01-01&to=2026-01-31
```

**Response (200):**
```json
{
  "data": {
    "total_revenue_cents": 4500000,
    "paid_cents": 4200000,
    "pending_cents": 300000,
    "refunded_cents": 50000,
    "by_payment_status": {
      "paid": 280,
      "pending": 20,
      "refunded": 3
    },
    "average_ticket_cents": 15000
  }
}
```

---

### 10. Webhooks

#### POST `/webhooks`
**Descrição:** Registrar webhook para eventos.

**Request:**
```json
{
  "url": "https://meu-sistema.com/webhook",
  "events": [
    "appointment.created",
    "appointment.cancelled",
    "payment.received"
  ],
  "secret": "webhook-secret-key"
}
```

**Response (201):** Webhook registrado

---

#### Webhook Payload Example
```json
{
  "event": "appointment.created",
  "timestamp": "2026-01-05T10:30:00Z",
  "data": {
    "appointment_id": "appt-uuid",
    "customer_id": "customer-uuid",
    "scheduled_at": "2026-01-10T14:00:00Z"
  },
  "signature": "sha256=abc123def456..."
}
```

**Signature Verification (HMAC SHA256):**
```csharp
var payload = await Request.ReadAsStringAsync();
var signature = Request.Headers["X-Webhook-Signature"];
var computedSignature = ComputeHmac(payload, webhookSecret);
if (signature != computedSignature) throw new UnauthorizedException();
```

---

## Pagination

### Cursor-based (recomendado para feeds infinitos)
**Request:**
```
GET /appointments?cursor=eyJpZCI6ImFiYy0xMjMiLCJkYXRlIjoiMjAyNi0wMS0xMCJ9&limit=20
```

**Response:**
```json
{
  "data": [...],
  "meta": {
    "next_cursor": "eyJpZCI6Inh5ei03ODkiLCJkYXRlIjoiMjAyNi0wMS0wOSJ9",
    "has_more": true
  }
}
```

### Offset-based (admin panels)
**Request:**
```
GET /customers?page=2&limit=50
```

**Response:**
```json
{
  "data": [...],
  "meta": {
    "page": 2,
    "limit": 50,
    "total": 234,
    "total_pages": 5
  }
}
```

---

## Filtering & Sorting

### Operators
```
?status=scheduled,confirmed          // IN operator
?price_cents[gte]=10000               // Greater than or equal
?created_at[lt]=2026-01-01            // Less than
?full_name[like]=maria                // LIKE operator (fuzzy search)
```

### Sorting
```
?sort=scheduled_at:desc,created_at:asc
```

---

## Caching

### ETags
**Request:**
```
GET /appointments/:id
If-None-Match: "abc123def456"
```

**Response (304 Not Modified)** se ETag não mudou  
**Response (200)** com novo ETag se mudou

### Cache-Control Headers
```http
Cache-Control: public, max-age=60, stale-while-revalidate=300
```

---

## Idempotency

**Header:**
```http
Idempotency-Key: unique-key-per-request
```

**Uso:**
- Prevenir criação duplicada em caso de retry
- Backend armazena key + resultado por 24h no Redis
- Se request repetido com mesma key, retorna resultado cached

---

## Versioning Strategy

### URL-based (escolhido)
```
/api/v1/appointments
/api/v2/appointments  (quando houver breaking change)
```

### Deprecation
```http
Sunset: Sat, 01 Jan 2027 00:00:00 GMT
Link: <https://docs.pilar1.app/migrations/v2>; rel="deprecation"
```

---

## SDK Examples

### C# Client
```csharp
var client = new AstraFutureClient("https://api.astrafuture.app/api/v1", jwtToken);

var appointment = await client.Appointments.CreateAsync(new CreateAppointmentRequest
{
    CustomerId = Guid.Parse("..."),
    ResourceId = Guid.Parse("..."),
    ScheduledAt = DateTime.Parse("2026-01-10T14:00:00Z"),
    DurationMinutes = 60,
    Title = "Primeira Consulta"
});
```

### TypeScript/JavaScript Client
```typescript
const client = new AstraFutureClient('https://api.astrafuture.app/api/v1', jwtToken);

const appointment = await client.appointments.create({
  customer_id: 'customer-uuid',
  resource_id: 'resource-uuid',
  scheduled_at: '2026-01-10T14:00:00Z',
  duration_minutes: 60,
  title: 'Primeira Consulta'
});
```

---

## OpenAPI Specification

**Arquivo completo:** [openapi.yaml](./openapi.yaml)

```yaml
openapi: 3.0.0
info:
  title: AstraFuture API
  version: 1.0.0
  description: API universal para agendamento e gestão de serviços premium

servers:
  - url: https://api.astrafuture.app/api/v1
    description: Production
  - url: https://api-staging.astrafuture.app/api/v1
    description: Staging

paths:
  /appointments:
    get:
      summary: List appointments
      tags: [Appointments]
      security:
        - BearerAuth: []
      parameters:
        - in: query
          name: from
          schema:
            type: string
            format: date-time
      responses:
        '200':
          description: Success
          content:
            application/json:
              schema:
                type: object
                properties:
                  data:
                    type: array
                    items:
                      $ref: '#/components/schemas/Appointment'
# ... (continua)
```

---

## Performance Best Practices

### Backend
1. **N+1 Prevention**: Usar `Include` ou `Select` para eager loading
2. **Query Optimization**: Sempre filtrar por `tenant_id` primeiro
3. **Connection Pooling**: Max 100 conexões por instância
4. **Background Jobs**: Operações pesadas (relatórios) em fila

### Frontend
1. **Data Fetching**: React Query com `staleTime` de 5min
2. **Pagination**: Infinite scroll com `useInfiniteQuery`
3. **Optimistic Updates**: Atualizar UI antes da API responder
4. **Debouncing**: Search com debounce de 300ms

---

**Próximo:** [WhatsApp Onboarding Flow](../workflows/whatsapp-onboarding.json)
