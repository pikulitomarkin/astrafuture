# Database Schema Documentation

## Visão Geral

O schema foi projetado para suportar arquitetura **multi-tenant** com isolamento via **Row-Level Security (RLS)**, permitindo escala horizontal infinita sem overhead de schemas separados.

## Diagrama de Entidades (ER)

```
┌──────────────┐
│   TENANTS    │───┐
└──────────────┘   │
       │           │
       ├───────────┼─────────────┐
       │           │             │
       ▼           ▼             ▼
┌──────────┐ ┌───────────┐ ┌────────────┐
│  USERS   │ │ RESOURCES │ │ CUSTOMERS  │
└──────────┘ └───────────┘ └────────────┘
       │           │             │
       └───────────┼─────────────┘
                   │
                   ▼
         ┌──────────────────┐
         │  APPOINTMENTS    │
         └──────────────────┘
                   │
         ┌─────────┼──────────┐
         ▼         ▼          ▼
    ┌─────────┐ ┌────────┐ ┌──────────┐
    │ HISTORY │ │ NOTIFS │ │ WEBHOOKS │
    └─────────┘ └────────┘ └──────────┘
```

## Tabelas Core

### 1. `tenants`
**Propósito:** Cada organização cliente é um tenant isolado.

**Campos Críticos:**
- `slug` (único): URL-friendly identifier (ex: `clinica-psique`)
- `tenant_type`: Define especialização ('psychology', 'law', 'construction')
- `meta_schema`: JSON Schema dos campos customizados
- `business_rules`: Regras específicas do negócio em JSON

**Exemplo de business_rules:**
```json
{
  "min_duration_minutes": 50,
  "allow_online_sessions": true,
  "require_consent_form": true,
  "cancellation_policy_hours": 24,
  "auto_confirm_appointments": false
}
```

**Exemplo de meta_schema:**
```json
{
  "appointment": {
    "anamnesis": {
      "type": "richtext",
      "label": "Anamnese",
      "required": true,
      "visible_to": ["admin", "professional"]
    },
    "session_type": {
      "type": "select",
      "label": "Tipo de Sessão",
      "options": ["Inicial", "Retorno", "Grupo"],
      "required": true
    }
  },
  "customer": {
    "emergency_contact": {
      "type": "object",
      "properties": {
        "name": {"type": "string"},
        "phone": {"type": "string"}
      }
    }
  }
}
```

---

### 2. `users`
**Propósito:** Usuários internos do sistema (profissionais, admins).

**Sincronização com Supabase Auth:**
- `auth_user_id` referencia `auth.users.id` do Supabase
- Autenticação delegada ao Supabase (JWT)
- Backend valida token e extrai `auth_user_id` + `tenant_id`

**Hierarquia de Roles:**
```
owner (1 por tenant)
  └─ admin (N)
      └─ member (N)
          └─ guest (N)
```

**Permissions (exemplo):**
```json
[
  "appointments:read",
  "appointments:write",
  "appointments:delete",
  "customers:read",
  "customers:write",
  "reports:read",
  "settings:write"
]
```

---

### 3. `resources`
**Propósito:** Recursos agendáveis (profissionais, salas, equipamentos).

**Tipos:**
- `professional`: Médico, psicólogo, advogado (vinculado a `users.id`)
- `room`: Sala de atendimento, consultório
- `equipment`: Aparelho, veículo (ex: escavadeira para construção)

**working_hours (exemplo):**
```json
{
  "monday": [
    {"start": "09:00", "end": "12:00"},
    {"start": "14:00", "end": "18:00"}
  ],
  "tuesday": [
    {"start": "09:00", "end": "18:00"}
  ],
  "wednesday": [],
  "thursday": [
    {"start": "08:00", "end": "12:00"}
  ],
  "friday": [
    {"start": "09:00", "end": "17:00"}
  ],
  "saturday": [],
  "sunday": []
}
```

**booking_buffer_minutes:**
- Tempo de "respiro" entre appointments
- Ex: 10 minutos para higienização de sala

---

### 4. `customers`
**Propósito:** Clientes finais que agendam serviços.

**Identificação:**
- `phone` é obrigatório (principal canal de contato via WhatsApp)
- `email` é opcional (nem todo cliente tem email)
- `document_number` armazena CPF/CNPJ

**Lead Source Tracking:**
- Rastreia origem do cliente: `whatsapp`, `website`, `instagram`, `referral`
- `referred_by` permite programa de indicação

**Tags (Array):**
- Permite categorização livre: `['vip', 'inadimplente', 'primeira-consulta']`
- Usa GIN index para busca eficiente

---

### 5. `appointments` ⭐ (Núcleo do Sistema)
**Propósito:** Tabela central de agendamentos.

**Status Flow:**
```
    ┌─────────────┐
    │  scheduled  │ (agendado)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │  confirmed  │ (confirmado pelo cliente)
    └──────┬──────┘
           │
           ▼
    ┌─────────────┐
    │ in_progress │ (atendimento em andamento)
    └──────┬──────┘
           │
      ┌────┴────┐
      ▼         ▼
┌──────────┐ ┌─────────┐
│completed │ │cancelled│
└──────────┘ └─────────┘
      │         │
      ▼         ▼
   [FIM]  ┌─────────┐
          │ no_show │ (faltou)
          └─────────┘
```

**Campos Computados:**
- `ends_at`: GENERATED ALWAYS AS (`scheduled_at` + `duration_minutes`)
- Facilita queries de conflito de horário

**Prevenção de Conflitos:**
- Function `check_appointment_conflict()` valida sobreposição
- Considera `booking_buffer_minutes` do resource

**Payment Tracking:**
- `price_cents`: Sempre em centavos (evita float)
- `payment_status`: Integra com gateway de pagamento

---

### 6. `appointment_history`
**Propósito:** Auditoria completa de mudanças nos appointments.

**Casos de Uso:**
- Rastrear reagendamentos
- Identificar padrões de cancelamento
- Compliance (LGPD/GDPR)

---

### 7. `availability_exceptions`
**Propósito:** Exceções de disponibilidade (feriados, férias, horários especiais).

**Tipos:**
- `unavailable`: Profissional indisponível (férias, doença)
- `holiday`: Feriado nacional/local
- `special_hours`: Horário diferenciado

**Recurrence:**
- Suporta eventos recorrentes via RRULE (RFC 5545)
- Ex: "Todo primeiro sábado do mês"

---

### 8. `notifications`
**Propósito:** Fila de notificações multi-canal.

**Canais:**
- `email`: Confirmação, lembrete
- `sms`: Lembrete urgente
- `whatsapp`: Principal canal (via Evolution API)
- `push`: Notificação mobile
- `in_app`: Notificação no dashboard

**Retry Strategy:**
- Até 3 tentativas com exponential backoff
- Status: `pending` → `sent` → `delivered` / `failed`

---

## Row-Level Security (RLS)

### Conceito
Cada query automaticamente filtra por `tenant_id` através de políticas no nível de linha do PostgreSQL.

### Configuração no Backend

**C# Middleware (Supabase):**
```csharp
public class TenantContextMiddleware
{
    public async Task InvokeAsync(HttpContext context, SupabaseClient supabase)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();
        
        var jwtPayload = ValidateJwt(token);
        var tenantId = jwtPayload["tenant_id"];
        
        // Define contexto para RLS
        await supabase.Rpc("set_config", new {
            setting = "app.tenant_id",
            value = tenantId,
            is_local = true
        });
        
        await _next(context);
    }
}
```

**PostgreSQL Session:**
```sql
-- Backend define no início de cada transação
SET LOCAL app.tenant_id = '11111111-1111-1111-1111-111111111111';

-- Todas as queries subsequentes são filtradas automaticamente
SELECT * FROM appointments; -- Retorna apenas do tenant atual
```

### Vantagens
✅ **Segurança by Design**: Impossível acessar dados de outro tenant  
✅ **Zero Boilerplate**: Não precisa adicionar `WHERE tenant_id = ...` em cada query  
✅ **Performance**: Indexes com tenant_id são automaticamente utilizados  

---

## Indexes Críticos para Performance

### Composite Indexes
```sql
-- Query mais comum: "Listar appointments do tenant X em ordem cronológica"
CREATE INDEX idx_appointments_tenant_date 
ON appointments(tenant_id, scheduled_at DESC) 
WHERE deleted_at IS NULL;

-- Query: "Verificar conflito de horário para resource Y"
CREATE INDEX idx_appointments_resource_date 
ON appointments(resource_id, scheduled_at);

-- Query: "Buscar appointments por status"
CREATE INDEX idx_appointments_status 
ON appointments(tenant_id, status) 
WHERE deleted_at IS NULL;
```

### Full-Text Search
```sql
-- Busca em appointments por título/descrição
CREATE INDEX idx_appointments_search 
ON appointments USING GIN(
    to_tsvector('portuguese', 
        COALESCE(title, '') || ' ' || COALESCE(description, '')
    )
);

-- Query exemplo
SELECT * FROM appointments
WHERE to_tsvector('portuguese', title || ' ' || description) 
      @@ to_tsquery('portuguese', 'consulta & urgente');
```

### Partial Indexes
```sql
-- Índice apenas para registros ativos (economiza espaço)
CREATE INDEX idx_users_active 
ON users(tenant_id, email) 
WHERE is_active = true AND deleted_at IS NULL;
```

---

## Functions Utilitárias

### 1. `check_appointment_conflict()`
**Objetivo:** Validar se novo appointment causa conflito.

**Lógica:**
1. Busca appointments existentes do mesmo `resource_id`
2. Verifica sobreposição de horários:
   - Novo começa durante existente
   - Novo termina durante existente
   - Novo engloba existente completamente
3. Considera `booking_buffer_minutes`

**Uso:**
```sql
SELECT check_appointment_conflict(
    'resource-uuid',
    '2026-01-10 14:00:00',
    60, -- duration
    NULL -- exclude_appointment_id (para updates)
);
-- Retorna: true se conflita, false se disponível
```

---

### 2. `get_available_slots()`
**Objetivo:** Retornar horários disponíveis para agendamento.

**Lógica Complexa:**
1. Obter `working_hours` do resource
2. Subtrair `availability_exceptions` (feriados, férias)
3. Subtrair appointments existentes
4. Adicionar `booking_buffer_minutes`
5. Retornar slots de X minutos no intervalo disponível

**Exemplo de Retorno:**
```
slot_time             | is_available
----------------------|-------------
2026-01-10 09:00:00  | true
2026-01-10 10:00:00  | true
2026-01-10 11:00:00  | false (ocupado)
2026-01-10 12:00:00  | false (almoço)
2026-01-10 14:00:00  | true
```

---

## Views Otimizadas

### `appointments_detailed`
**Propósito:** JOIN pré-computado para dashboard.

**Performance:**
- Evita múltiplos JOINs em cada query
- Pode ser materializada se necessário (MATERIALIZED VIEW)

**Uso:**
```sql
SELECT * FROM appointments_detailed
WHERE tenant_id = current_tenant_id()
AND scheduled_at BETWEEN '2026-01-01' AND '2026-01-31';
```

---

### `tenant_statistics`
**Propósito:** Métricas agregadas do tenant.

**Campos:**
- `total_users`, `total_customers`
- `appointments_last_30_days`
- `revenue_last_30_days_cents`

**Uso em Dashboard:**
```typescript
// Frontend busca estatísticas agregadas
const stats = await api.get('/tenants/me/statistics');
// Retorna dados da view pré-computada
```

---

## Estratégias de Particionamento (Futuro)

Quando atingir **10M+ appointments**:

### Range Partitioning por Data
```sql
CREATE TABLE appointments_2026_01 
PARTITION OF appointments 
FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');

CREATE TABLE appointments_2026_02 
PARTITION OF appointments 
FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
```

**Vantagens:**
- Queries filtradas por data acessam apenas partição relevante
- Arquivamento simplificado (DROP PARTITION antiga)

---

## Migrations Strategy

### Versionamento
```
database/
  migrations/
    001_initial_schema.sql
    002_add_meta_fields.sql
    003_add_notifications_table.sql
```

### Aplicação
```bash
# Usando Flyway, Liquibase ou custom script
psql -U pilar1 -d pilar1_prod -f migrations/001_initial_schema.sql
```

### Rollback Safety
- Sempre incluir `-- ROLLBACK` statements
- Testar em staging antes de produção
- Manter backups antes de migrations críticas

---

## Backup & Disaster Recovery

### Daily Backups
```bash
# Automated via cron
pg_dump -h localhost -U pilar1 -Fc pilar1_prod > backup_$(date +%Y%m%d).dump
```

### Point-in-Time Recovery (PITR)
- Configurar WAL archiving
- Permite restaurar estado exato até segundo específico

### Replication
```
Primary (Write) → Streaming Replication → Standby (Read)
```

---

## Compliance & LGPD

### Direito ao Esquecimento
```sql
-- Anonimizar dados do customer
UPDATE customers 
SET 
    full_name = 'Usuário Anônimo',
    email = NULL,
    phone = 'REDACTED',
    document_number = NULL,
    address = '{}',
    meta_data = '{}'
WHERE id = 'customer-uuid';
```

### Auditoria
- Tabela `audit_logs` registra TODAS as ações
- Retention de 7 anos (compliance)
- Exportação via CSV para auditorias externas

---

## Perguntas Frequentes (FAQ)

**Q: Por que não usar UUID v7 em vez de v4?**  
A: UUID v7 tem ordenação temporal, melhorando performance de INSERT. Considerar migração futura.

**Q: E se um tenant crescer muito (1M+ appointments)?**  
A: Particionar tabela por range de data. RLS continua funcionando normalmente.

**Q: Como lidar com timezones?**  
A: Sempre armazenar em UTC (TIMESTAMPTZ). Converter para timezone do tenant no frontend.

**Q: Suporta multi-location (franquias)?**  
A: Sim. Adicionar tabela `locations` com FK para `tenant_id`. Resources vinculam-se a location.

---

**Próximo:** [API Specification](../api/openapi.yaml)
