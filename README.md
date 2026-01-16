# 🚀 AstraFuture - Sistema de Agendamentos Multi-Tenant

> **Status:** ✅ Semana 1 Completa - 70% do MVP Implementado  
> **Última atualização:** 16 Janeiro 2026

---

## 📊 Status do Projeto

| Componente | Status | Progresso |
|-----------|--------|-----------|
| Backend API | ✅ Completo | 100% |
| Frontend Base | ✅ Completo | 100% |
| Autenticação | ✅ Completo | 100% |
| CRUD Appointments | 🟡 Parcial | 50% |
| Deploy | ⏸️ Pendente | 0% |
| **TOTAL MVP** | 🟡 Em Progresso | **70%** |

---

## 📋 Documentação Rápida

### 🏃‍♂️ Setup Rápido
- **[SETUP-FRONTEND.md](./SETUP-FRONTEND.md)** - Como rodar o frontend em 5 minutos
- **[SETUP-SUPABASE.md](./SETUP-SUPABASE.md)** - Configurar database e auth

### 📈 Progresso
- **[ENTREGA-SEMANA-1.md](./ENTREGA-SEMANA-1.md)** - Resumo completo da Semana 1
- **[PROGRESSO-DIA-5.md](./PROGRESSO-DIA-5.md)** - Detalhes do último dia
- **[PLANO-EXECUCAO.md](./PLANO-EXECUCAO.md)** - Roadmap completo (11 dias)

### 🏗️ Arquitetura
- **[architecture/00-OVERVIEW.md](./architecture/00-OVERVIEW.md)** - Visão geral técnica
- **[database/schema.sql](./database/schema.sql)** - Schema PostgreSQL com RLS

---

## 🚀 Como Executar

### Pré-requisitos
- ✅ .NET 9.0 SDK
- ✅ Node.js 18+
- ✅ Conta Supabase (grátis)

### Backend (.NET)
```bash
cd backend-src/AstraFuture.Api
dotnet run
```
Backend em: `http://localhost:5000`

### Frontend (Next.js)
```bash
cd frontend
npm install
npm run dev
```
Frontend em: `http://localhost:3000`

### Detalhes Completos
Veja [SETUP-FRONTEND.md](./SETUP-FRONTEND.md) para instruções detalhadas.

---

## 📊 Visão Geral do Projeto

**AstraFuture** é uma plataforma SaaS universal e agnóstica para agendamento e prestação de serviços premium, projetada para escalar infinitamente através de arquitetura multi-tenant.

### Características Principais

✅ **Multi-tenancy com RLS** - Isolamento garantido via Row-Level Security  
✅ **Core Agnóstico** - Funciona para qualquer nicho (psicologia, advocacia, construção)  
✅ **Onboarding Zero Burocracia** - De "Oi" no WhatsApp a dashboard em < 3 minutos  
✅ **UX Premium** - Micro-interações, optimistic updates, zero formulários longos  
✅ **Baixa Latência** - Cache em múltiplas camadas (Edge, Redis, Browser)  

---

## 🏛️ Arquitetura em Alto Nível

```
┌─────────────────────────────────────────────────────────────┐
│                        FRONTEND                              │
│  Next.js 14 + React + TypeScript + TailwindCSS + ShadcnUI  │
│                    (Vercel Edge Network)                     │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTPS/REST
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                     API GATEWAY                              │
│         Rate Limiting + JWT Validation + CORS               │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│                    BACKEND API                               │
│        .NET 8 + ASP.NET Core + Clean Architecture           │
│                     (Fly.io / AWS)                           │
└──┬──────────────────┬──────────────────┬────────────────────┘
   │                  │                  │
   ▼                  ▼                  ▼
┌────────┐      ┌─────────┐       ┌──────────┐
│ Supabase│      │  Redis  │       │   n8n    │
│PostgreSQL│     │  Cache  │       │Automation│
│   +RLS  │      │         │       │          │
└────────┘      └─────────┘       └──────────┘
```

---

## 📁 Estrutura do Monorepo (Proposta)

```
astrafuture/
├── backend/                      # .NET 8 Backend
│   ├── AstraFuture.Api/              # Web API (Controllers, Middleware)
│   ├── AstraFuture.Application/      # Use Cases, DTOs, Interfaces
│   ├── AstraFuture.Domain/           # Entities, Value Objects, Domain Events
│   ├── AstraFuture.Infrastructure/   # Data Access, External Services
│   └── AstraFuture.Shared/           # Cross-cutting concerns
│
├── frontend/                     # Next.js 14 Frontend
│   ├── src/
│   │   ├── app/                 # App Router (pages)
│   │   ├── components/          # UI Components
│   │   ├── lib/                 # Utilities, API client
│   │   └── hooks/               # Custom hooks
│   ├── public/
│   └── package.json
│
├── database/                     # Database artifacts
│   ├── schema.sql               # PostgreSQL schema
│   ├── migrations/              # Migration scripts
│   └── seeds/                   # Seed data
│
├── workflows/                    # n8n workflows
│   ├── whatsapp-onboarding.json
│   └── notifications.json
│
├── docs/                         # Documentation
│   ├── architecture/
│   ├── api/
│   └── ux-strategy.md
│
├── infrastructure/               # IaC (Terraform/Pulumi)
│   ├── terraform/
│   └── docker-compose.yml
│
└── README.md
```

---

## 🛠️ Stack Tecnológica Detalhada

### Backend
```yaml
Runtime: .NET 8 (LTS)
Framework: ASP.NET Core Web API
Architecture: Clean Architecture + CQRS Light
ORM: Dapper (queries) + EF Core (migrations)
Auth: Supabase Auth (JWT)
Validation: FluentValidation
Logging: Serilog → Seq
Testing: xUnit + FluentAssertions + Testcontainers
```

### Frontend
```yaml
Framework: Next.js 14.1+
Language: TypeScript 5.3+
UI: ShadcnUI + Tailwind CSS 3.4+
Forms: React Hook Form + Zod
Data Fetching: React Query v5
Animation: Framer Motion
State: Zustand (global) + Context (local)
```

### Infraestrutura
```yaml
Database: PostgreSQL 15+ (Supabase)
Cache: Redis 7+ (Upstash)
Storage: Supabase Storage (S3-compatible)
CDN: Cloudflare
Hosting Backend: Fly.io
Hosting Frontend: Vercel
Monitoring: Grafana + Prometheus + Sentry
```

### DevOps
```yaml
CI/CD: GitHub Actions
Containers: Docker + Docker Compose
Secrets: Doppler
IaC: Terraform (opcional)
```

---

## 🔐 Segurança

### Row-Level Security (RLS)
Todas as queries são automaticamente filtradas por `tenant_id` através de políticas PostgreSQL.

**Exemplo:**
```sql
-- Usuários só veem appointments do próprio tenant
CREATE POLICY "Tenant isolation" ON appointments
FOR SELECT USING (tenant_id = current_setting('app.tenant_id')::uuid);
```

### Autenticação
- **JWT Tokens** emitidos pelo Supabase Auth
- **Claims**: `user_id`, `tenant_id`, `role`, `permissions`
- **Refresh Tokens** com rotação automática

### Rate Limiting
| Tier | Requests/min | Requests/dia |
|------|--------------|--------------|
| Free | 100 | 10,000 |
| Pro | 1,000 | 100,000 |
| Enterprise | Custom | Custom |

---

## 📈 Performance

### Targets
- **API Latency P95**: < 200ms
- **First Contentful Paint**: < 1.5s
- **Time to Interactive**: < 3s
- **Lighthouse Score**: > 90

### Estratégias
- ✅ Optimistic Updates (UI instantânea)
- ✅ Prefetching (React Query)
- ✅ Edge Caching (Vercel/Cloudflare)
- ✅ Redis para query results (TTL 5min)
- ✅ Connection Pooling (PgBouncer)

---

## 🧪 Testing Strategy

### Backend
```bash
# Unit tests (Domain + Application)
dotnet test --filter Category=Unit

# Integration tests (API + Database)
dotnet test --filter Category=Integration

# E2E tests (usando Testcontainers)
dotnet test --filter Category=E2E
```

### Frontend
```bash
# Unit tests (Components)
npm run test

# E2E tests (Playwright)
npm run test:e2e

# Visual regression (Chromatic)
npm run test:visual
```

### Coverage Target
- Backend: > 80%
- Frontend: > 70%

---

## 🚢 Deployment

### Ambientes

```
Development → localhost (Docker Compose)
Staging     → staging.astrafuture.app
Production  → app.astrafuture.app
```

### CI/CD Pipeline

```yaml
# .github/workflows/deploy.yml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - run: dotnet test
      - run: npm test
  
  deploy-backend:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: superfly/flyctl-actions@v1
        with:
          args: deploy --config backend/fly.toml
  
  deploy-frontend:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: amondnet/vercel-action@v25
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}
```

---

## 📚 Próximos Passos

### Fase 1: MVP (4 semanas)
- [ ] Setup da infraestrutura (database, auth, hosting)
- [ ] Backend: Endpoints core (tenants, users, appointments)
- [ ] Frontend: Dashboard básico + Calendar
- [ ] Onboarding: Flow WhatsApp (n8n)

### Fase 2: Premium Features (4 semanas)
- [ ] UX: Micro-interações + Optimistic updates
- [ ] Notificações multi-canal (email, SMS, WhatsApp)
- [ ] Analytics & Relatórios
- [ ] Payment integration (Stripe)

### Fase 3: Scale (4 semanas)
- [ ] Performance optimization
- [ ] Multi-location support
- [ ] Advanced business rules engine
- [ ] Mobile app (React Native)

---

## 🤝 Contributing

### Workflow
1. Fork o repositório
2. Crie uma branch (`git checkout -b feature/nova-feature`)
3. Commit suas mudanças (`git commit -m 'Add nova feature'`)
4. Push para a branch (`git push origin feature/nova-feature`)
5. Abra um Pull Request

### Code Style
- Backend: [Microsoft C# Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Frontend: [Airbnb JavaScript Style Guide](https://github.com/airbnb/javascript)

---

## 📞 Suporte

- **Email**: dev@astrafuture.app
- **Discord**: [discord.gg/astrafuture](https://discord.gg/astrafuture)
- **Docs**: [docs.astrafuture.app](https://docs.astrafuture.app)

---

## 📄 Licença

MIT License - veja [LICENSE](LICENSE) para detalhes.

---

**Desenvolvido com ❤️ para transformar agendamentos em experiências premium.**
