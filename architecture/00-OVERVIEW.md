# AstraFuture - Arquitetura SaaS Multi-tenant Universal

## Visão Executiva

Plataforma SaaS agnóstica para agendamento e prestação de serviços premium, projetada para escala infinita através de arquitetura multi-tenant isolada. O core é independente de nicho, permitindo especialização através de camadas de regras de negócio dinâmicas.

---

## Decisões Arquiteturais Fundamentais

### 1. Multi-tenancy Strategy: Row-Level Security (RLS)

**Decisão:** Utilizar Supabase com PostgreSQL + Row-Level Security (RLS)

**Justificativa:**
- ✅ **Escalabilidade Horizontal**: Single database com particionamento lógico via RLS
- ✅ **Isolamento Garantido**: Políticas de segurança no nível de linha impedem cross-tenant data leakage
- ✅ **Custo-Efetivo**: Elimina overhead de schema-per-tenant ou database-per-tenant
- ✅ **Simplicidade Operacional**: Backups, migrations e manutenção centralizados
- ✅ **Performance**: Indexes compartilhados + connection pooling

**Alternativa Considerada (Descartada):**
- Schema-per-tenant: Maior complexidade operacional e limite de escalabilidade (PostgreSQL tem limite prático de ~10k schemas)

### 2. Backend Architecture: Clean Architecture com .NET 8

**Estrutura em Camadas:**

```
AstraFuture.Api/                    ← Presentation Layer (Controllers, Middlewares)
AstraFuture.Application/            ← Use Cases, DTOs, Interfaces
AstraFuture.Domain/                 ← Entities, Value Objects, Domain Events
AstraFuture.Infrastructure/         ← Data Access, External Services, Cache
AstraFuture.Shared/                 ← Cross-cutting concerns (Logging, Validation)
```

**Princípios:**
- **Dependency Inversion**: Domain não depende de nada
- **CQRS Light**: Separação entre Commands (escrita) e Queries (leitura)
- **Repository Pattern**: Abstração de acesso a dados
- **Unit of Work**: Transações atômicas cross-aggregate

### 3. Frontend Architecture: Next.js 14 (App Router)

**Stack:**
- **Framework**: Next.js 14 com TypeScript
- **UI Components**: ShadcnUI + Tailwind CSS
- **State Management**: Zustand (global) + React Query (server state)
- **Validação**: Zod + React Hook Form
- **Edge Rendering**: Server Components para SEO e performance

**Pattern: Atomic Design Modificado**
```
/components
  /atoms          ← Button, Input, Badge
  /molecules      ← FormField, Card, DatePicker
  /organisms      ← AppointmentCard, CalendarGrid
  /templates      ← DashboardLayout, BookingFlow
```

### 4. Orquestração: n8n Self-hosted

**Casos de Uso:**
- ✅ Onboarding via WhatsApp (Evolution API)
- ✅ Notificações multi-canal (Email, SMS, Push)
- ✅ Automação de status (lembrete de consulta, follow-up pós-atendimento)
- ✅ Integrações externas (Stripe, Google Calendar, Analytics)

**Vantagens sobre alternativas:**
- Open-source e self-hosted (controle total)
- Visual workflow editor (produto não técnico pode editar)
- 400+ integrações nativas

### 5. Caching Strategy: Redis + Edge Functions

**Níveis de Cache:**

| Camada | TTL | Invalidação |
|--------|-----|-------------|
| **Edge Functions** (Vercel/Cloudflare) | 60s | Stale-while-revalidate |
| **Redis (Query Results)** | 5min | Tag-based invalidation |
| **Redis (Session)** | 24h | On logout |
| **Browser (Static Assets)** | 1 year | Content-based hashing |

**Pattern: Cache-Aside com Fallback**
```
Request → Edge Cache → Redis → PostgreSQL
            ↓ miss      ↓ miss     ↓ hit
          (fetch) → (fetch) → (return)
```

---

## Stack Tecnológica Detalhada

### Backend
```yaml
Runtime: .NET 8 (LTS)
Framework: ASP.NET Core Web API
ORM: Dapper (queries) + EF Core (migrations)
Auth: Supabase Auth (JWT) + Custom claims
Validation: FluentValidation
Logging: Serilog → Seq/Grafana Loki
Testing: xUnit + FluentAssertions + Testcontainers
```

### Frontend
```yaml
Runtime: Node.js 20 LTS
Framework: Next.js 14.1+
Language: TypeScript 5.3+
UI: ShadcnUI + Tailwind CSS 3.4+
Forms: React Hook Form + Zod
Data Fetching: React Query v5
Animation: Framer Motion
Date/Time: date-fns
Charts: Recharts
```

### Infraestrutura
```yaml
Database: PostgreSQL 15+ (Supabase)
Cache: Redis 7+ (Upstash)
Storage: Supabase Storage (S3-compatible)
CDN: Cloudflare (DNS + Cache + WAF)
Hosting: 
  - Backend: Fly.io (auto-scaling)
  - Frontend: Vercel (Edge Network)
Monitoring: Grafana + Prometheus + Sentry
```

### DevOps
```yaml
CI/CD: GitHub Actions
Containers: Docker + Docker Compose
Secrets: Doppler ou Infisical
IaC: Terraform (opcional para infra complexa)
```

---

## Padrões de Segurança

### 1. Autenticação & Autorização

**Flow:**
```
Client → Supabase Auth (Magic Link/OAuth) → JWT → API Gateway
                                               ↓
                                        Validate Token
                                               ↓
                                        Extract tenant_id
                                               ↓
                                        Set RLS Context
```

**Claims do JWT:**
```json
{
  "sub": "user_uuid",
  "tenant_id": "tenant_uuid",
  "role": "admin|member|guest",
  "permissions": ["appointments:read", "appointments:write"],
  "exp": 1640995200
}
```

### 2. Row-Level Security (Exemplo)

```sql
-- Política para leitura de appointments
CREATE POLICY "Users can view appointments from their tenant"
ON appointments FOR SELECT
USING (tenant_id = current_setting('app.tenant_id')::uuid);

-- Política para escrita de appointments
CREATE POLICY "Users can insert appointments to their tenant"
ON appointments FOR INSERT
WITH CHECK (
  tenant_id = current_setting('app.tenant_id')::uuid
  AND created_by = auth.uid()
);
```

### 3. Rate Limiting

| Endpoint Type | Limite | Window |
|---------------|--------|--------|
| Authentication | 5 req | 1 min |
| API (Free Tier) | 100 req | 1 min |
| API (Pro Tier) | 1000 req | 1 min |
| Webhooks | 50 req | 1 min |

**Implementação:** Redis Sliding Window + API Gateway

---

## Estratégia de Especialização (Camada de Regras de Negócio)

### Conceito: Meta-Fields + Business Rules Engine

**Arquitetura:**
```
Core Universal (Pilar 1)
       ↓
   Meta-Fields (JSON Schema)
       ↓
Business Rules Engine (Rule-based)
       ↓
Tenant-Specific UI/UX
```

### Exemplo: Diferenciação por Nicho

#### Psicologia
```json
{
  "tenant_type": "psychology",
  "meta_fields": {
    "appointment": {
      "anamnesis": { "type": "richtext", "required": true },
      "session_type": { "type": "enum", "values": ["initial", "follow-up", "group"] },
      "insurance_provider": { "type": "string", "required": false }
    }
  },
  "business_rules": {
    "min_duration_minutes": 50,
    "allow_online_sessions": true,
    "require_consent_form": true
  }
}
```

#### Construção Civil
```json
{
  "tenant_type": "construction",
  "meta_fields": {
    "appointment": {
      "project_size_m2": { "type": "number", "required": true },
      "property_type": { "type": "enum", "values": ["residential", "commercial"] },
      "construction_phase": { "type": "string" }
    }
  },
  "business_rules": {
    "min_duration_minutes": 120,
    "allow_online_sessions": false,
    "require_site_visit": true
  }
}
```

**Implementação Backend:**
```csharp
public interface IBusinessRuleEngine
{
    Task<ValidationResult> ValidateAppointment(
        Appointment appointment, 
        TenantConfiguration config
    );
    
    Task<List<MetaField>> GetRequiredFields(
        string tenantType, 
        string entityType
    );
}
```

---

## Princípios de Performance

### 1. N+1 Query Prevention
- Utilizar `.Include()` do EF Core ou JOINs explícitos no Dapper
- GraphQL DataLoader pattern para batching

### 2. Pagination Strategy
```
Cursor-based pagination (preferencial para feeds infinitos)
Offset-based pagination (preferencial para admin panels)
```

### 3. Background Jobs
- **Hangfire** para jobs recorrentes (lembretes, relatórios)
- **RabbitMQ/Azure Service Bus** para mensageria assíncrona (opcional para escala extrema)

### 4. Database Indexes Críticos
```sql
CREATE INDEX idx_appointments_tenant_date 
ON appointments(tenant_id, scheduled_at) 
WHERE deleted_at IS NULL;

CREATE INDEX idx_users_tenant_email 
ON users(tenant_id, email) 
WHERE is_active = true;
```

---

## Observabilidade & Monitoring

### Métricas-Chave (SLIs)

| Métrica | Target | Alertar se |
|---------|--------|------------|
| API Latency P95 | < 200ms | > 500ms |
| Error Rate | < 0.1% | > 1% |
| Uptime | 99.9% | < 99.5% |
| Database Connection Pool | < 70% | > 85% |

### Logs Estruturados (Serilog)
```csharp
Log.Information(
    "Appointment created: {AppointmentId} for Tenant: {TenantId}",
    appointment.Id,
    appointment.TenantId
);
```

### Distributed Tracing
- OpenTelemetry para propagação de context (trace-id)
- Grafana Tempo para visualização de traces

---

## Modelo de Deployment

### Ambientes

```
Development  → feature branches → localhost
Staging      → main branch → staging.pilar1.app
Production   → tags v* → app.pilar1.app
```

### CI/CD Pipeline

```yaml
# GitHub Actions
on: push
jobs:
  test:
    - Run unit tests
    - Run integration tests (Testcontainers)
    - Code coverage > 80%
  
  build:
    - Docker build (multi-stage)
    - Push to registry
  
  deploy:
    - Terraform apply (if infra changed)
    - Deploy backend (Fly.io)
    - Deploy frontend (Vercel)
    - Run smoke tests
```

### Rollback Strategy
- **Blue-Green Deployment** no backend (zero-downtime)
- **Instant Rollback** no Vercel (frontend)

---

## Considerações de Escalabilidade

### Horizontal Scaling
- **Backend**: Stateless (JWT, Redis sessions) → Auto-scaling no Fly.io
- **Frontend**: Edge-first via Vercel/Cloudflare
- **Database**: Read replicas para queries pesadas (relatórios)

### Sharding Future-Proof
Quando atingir ~1M tenants:
```
Partition by: hash(tenant_id) % 16 → 16 logical shards
```

### Cost Optimization
- Arquivar appointments antigos (> 2 anos) para cold storage (S3 Glacier)
- Comprimir logs após 30 dias
- Lazy-load de meta-fields (não carregar todos os fields desnecessariamente)

---

## Referências Técnicas

- [The Twelve-Factor App](https://12factor.net/)
- [Microsoft Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [Supabase RLS Best Practices](https://supabase.com/docs/guides/auth/row-level-security)
- [Next.js App Router Guide](https://nextjs.org/docs/app)

---

**Próximos Documentos:**
1. [Database Schema](../database/schema.sql)
2. [API Specification](../api/openapi.yaml)
3. [WhatsApp Onboarding Flow](../workflows/whatsapp-onboarding.json)
4. [UX Strategy](../docs/ux-strategy.md)
