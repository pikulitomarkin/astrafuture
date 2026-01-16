# 🚀 AstraFuture - Roadmap de Implementação (3 Semanas)

**Meta:** MVP funcional em produção até **26 de janeiro de 2026**

---

## 📅 Semana 1 (5-11 Jan): Fundação & Backend Core

### 🎯 Objetivos da Semana
- ✅ Infraestrutura configurada e rodando
- ✅ Backend API com endpoints essenciais
- ✅ Autenticação funcionando
- ✅ Database com seed data

---

### Segunda (5 Jan) - Setup de Infraestrutura

#### Manhã: Supabase & Database
```bash
# 1. Criar projeto no Supabase
# - Acesse: https://supabase.com
# - New Project: "astrafuture-prod"
# - Região: South America (São Paulo)
# - Salvar: DATABASE_URL, ANON_KEY, SERVICE_ROLE_KEY

# 2. Executar schema
cd database
# Copiar conteúdo de schema.sql e executar no Supabase SQL Editor
# Ou via CLI:
supabase db push
```

**Checklist:**
- [ ] Projeto Supabase criado
- [ ] Schema executado (10+ tabelas criadas)
- [ ] RLS policies ativas
- [ ] Seed data inserido (1 tenant de exemplo)

#### Tarde: Backend - Criar Solução .NET

```bash
# Criar estrutura de projetos
cd backend

# API
dotnet new webapi -n AstraFuture.Api
cd AstraFuture.Api
dotnet add package Supabase --version 0.15.2
dotnet add package Dapper --version 2.1.28
dotnet add package FluentValidation.AspNetCore --version 11.3.0
dotnet add package MediatR --version 12.2.0
dotnet add package Serilog.AspNetCore --version 8.0.0

# Domain
cd ..
dotnet new classlib -n AstraFuture.Domain

# Application
dotnet new classlib -n AstraFuture.Application
cd AstraFuture.Application
dotnet add package MediatR --version 12.2.0
dotnet add package FluentValidation --version 11.9.0

# Infrastructure
cd ..
dotnet new classlib -n AstraFuture.Infrastructure
cd AstraFuture.Infrastructure
dotnet add package Supabase --version 0.15.2
dotnet add package Dapper --version 2.1.28
dotnet add package StackExchange.Redis --version 2.7.17

# Shared
cd ..
dotnet new classlib -n AstraFuture.Shared

# Criar solution
dotnet new sln -n AstraFuture
dotnet sln add AstraFuture.Api/AstraFuture.Api.csproj
dotnet sln add AstraFuture.Domain/AstraFuture.Domain.csproj
dotnet sln add AstraFuture.Application/AstraFuture.Application.csproj
dotnet sln add AstraFuture.Infrastructure/AstraFuture.Infrastructure.csproj
dotnet sln add AstraFuture.Shared/AstraFuture.Shared.csproj

# Adicionar referências
cd AstraFuture.Api
dotnet add reference ../AstraFuture.Application/AstraFuture.Application.csproj
dotnet add reference ../AstraFuture.Infrastructure/AstraFuture.Infrastructure.csproj

cd ../AstraFuture.Application
dotnet add reference ../AstraFuture.Domain/AstraFuture.Domain.csproj
dotnet add reference ../AstraFuture.Shared/AstraFuture.Shared.csproj

cd ../AstraFuture.Infrastructure
dotnet add reference ../AstraFuture.Domain/AstraFuture.Domain.csproj
dotnet add reference ../AstraFuture.Application/AstraFuture.Application.csproj
```

**Checklist:**
- [ ] Solução .NET criada
- [ ] Pacotes NuGet instalados
- [ ] Referências entre projetos configuradas
- [ ] Projeto compila sem erros

---

### Terça (6 Jan) - Domain & Entities

#### Implementar Entities Core

**Criar:** `AstraFuture.Domain/Entities/BaseEntity.cs`
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Criar entities (seguir exemplos em `backend/README.md`):**
- [ ] `Tenant.cs` (com factory method)
- [ ] `User.cs` (com roles e permissions)
- [ ] `Customer.cs`
- [ ] `Resource.cs`
- [ ] `Appointment.cs` ⭐ (mais importante)

**Value Objects:**
- [ ] `Email.cs`
- [ ] `Phone.cs`
- [ ] `Money.cs`

**Checklist:**
- [ ] 5 entities implementadas
- [ ] Factory methods criados
- [ ] Business logic no domain
- [ ] Testes unitários (mínimo 10)

---

### Quarta (7 Jan) - Infrastructure & Repositories

#### Implementar Data Access

**Criar:** `AstraFuture.Infrastructure/Data/SupabaseContext.cs`
```csharp
public class SupabaseContext
{
    private readonly Supabase.Client _client;
    private readonly ITenantContext _tenantContext;
    
    public SupabaseContext(IConfiguration config, ITenantContext tenantContext)
    {
        var url = config["Supabase:Url"];
        var key = config["Supabase:ServiceRoleKey"];
        
        _client = new Supabase.Client(url, key);
        _tenantContext = tenantContext;
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        // Set RLS context
        await _client.Rpc("set_config", new {
            setting = "app.tenant_id",
            value = _tenantContext.TenantId.ToString(),
            is_local = true
        });
        
        return await operation();
    }
}
```

**Implementar Repositories:**
- [ ] `IAppointmentRepository` (interface)
- [ ] `AppointmentRepository` (implementação com Dapper)
- [ ] `ICustomerRepository`
- [ ] `CustomerRepository`
- [ ] `ITenantRepository`
- [ ] `TenantRepository`

**Checklist:**
- [ ] 3+ repositories implementados
- [ ] RLS context funcionando
- [ ] Queries otimizadas (usar EXPLAIN ANALYZE)
- [ ] Testes de integração (Testcontainers)

---

### Quinta (8 Jan) - Application Layer & Use Cases

#### Implementar CQRS Commands & Queries

**Commands (Write):**
- [ ] `CreateAppointmentCommand` + Handler
- [ ] `RescheduleAppointmentCommand` + Handler
- [ ] `CompleteAppointmentCommand` + Handler
- [ ] `CancelAppointmentCommand` + Handler
- [ ] `CreateCustomerCommand` + Handler

**Queries (Read):**
- [ ] `ListAppointmentsQuery` + Handler
- [ ] `GetAppointmentByIdQuery` + Handler
- [ ] `GetAvailableSlotsQuery` + Handler
- [ ] `ListCustomersQuery` + Handler

**DTOs:**
- [ ] `AppointmentDto`
- [ ] `CustomerDto`
- [ ] `AvailableSlotDto`

**Validators (FluentValidation):**
- [ ] `CreateAppointmentCommandValidator`
- [ ] `CreateCustomerCommandValidator`

**Checklist:**
- [ ] 9+ use cases implementados
- [ ] Validação funcionando
- [ ] Error handling correto
- [ ] Testes unitários (coverage > 80%)

---

### Sexta (9 Jan) - API Controllers & Middleware

#### Implementar Controllers

**Criar Controllers:**
- [ ] `AppointmentsController` (CRUD completo - 7 endpoints)
- [ ] `CustomersController` (CRUD - 5 endpoints)
- [ ] `ResourcesController` (CRUD - 5 endpoints)
- [ ] `AvailabilityController` (slots - 2 endpoints)
- [ ] `AuthController` (register, login - 2 endpoints)

**Middleware:**
- [ ] `TenantContextMiddleware` (extrai tenant_id do JWT)
- [ ] `ExceptionHandlingMiddleware` (global error handler)
- [ ] `RequestLoggingMiddleware` (Serilog)

**Configuration:**
```csharp
// Program.cs
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAppointmentCommand).Assembly));
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
// ... outros serviços

app.UseMiddleware<TenantContextMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

**Checklist:**
- [ ] 5 controllers implementados
- [ ] 21+ endpoints funcionando
- [ ] Swagger configurado
- [ ] Middleware chain correto
- [ ] Testes de integração (API)

---

### Sábado-Domingo (10-11 Jan) - Auth & Deploy Backend

#### Configurar Autenticação Supabase

**Criar:** `AstraFuture.Api/Services/AuthService.cs`
```csharp
public class AuthService
{
    private readonly Supabase.Client _supabase;
    
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Criar tenant
        var tenant = Tenant.Create(request.TenantName, request.TenantType);
        await _tenantRepository.AddAsync(tenant);
        
        // 2. Criar user no Supabase Auth
        var authResult = await _supabase.Auth.SignUp(request.Email, GeneratePassword());
        
        // 3. Criar user no nosso DB
        var user = User.Create(
            tenant.Id,
            authResult.User.Id,
            request.Email,
            request.Phone,
            request.FullName,
            UserRole.Owner
        );
        await _userRepository.AddAsync(user);
        
        // 4. Gerar Magic Link
        var magicLink = await GenerateMagicLinkAsync(user.Id);
        
        return new AuthResponse { MagicLink = magicLink };
    }
}
```

**Deploy no Fly.io:**
```bash
# Instalar Fly CLI
# Windows: https://fly.io/docs/hands-on/install-flyctl/

# Login
flyctl auth login

# Criar app
flyctl launch --name astrafuture-api --region gru

# Configurar secrets
flyctl secrets set DATABASE_URL="postgresql://..."
flyctl secrets set SUPABASE_URL="https://..."
flyctl secrets set SUPABASE_ANON_KEY="..."
flyctl secrets set JWT_SECRET="..."

# Deploy
flyctl deploy
```

**Checklist:**
- [ ] AuthService implementado
- [ ] JWT validation funcionando
- [ ] Magic Link generation funcionando
- [ ] API deployada no Fly.io
- [ ] Health check retornando 200
- [ ] Testar endpoints via Postman/Insomnia

---

## 📅 Semana 2 (12-18 Jan): Frontend & UX

### 🎯 Objetivos da Semana
- ✅ Frontend Next.js configurado
- ✅ Dashboard básico funcionando
- ✅ Calendar component implementado
- ✅ CRUD de appointments funcionando

---

### Segunda (12 Jan) - Setup Frontend

#### Criar Projeto Next.js

```bash
cd ..
npx create-next-app@latest frontend --typescript --tailwind --app --use-npm
cd frontend

# Instalar dependências
npm install @tanstack/react-query
npm install zustand
npm install @supabase/supabase-js
npm install zod
npm install react-hook-form
npm install @hookform/resolvers
npm install framer-motion
npm install date-fns
npm install lucide-react

# ShadcnUI
npx shadcn-ui@latest init
npx shadcn-ui@latest add button
npx shadcn-ui@latest add card
npx shadcn-ui@latest add input
npx shadcn-ui@latest add dialog
npx shadcn-ui@latest add dropdown-menu
npx shadcn-ui@latest add calendar
npx shadcn-ui@latest add toast
npx shadcn-ui@latest add command
```

**Estrutura de pastas:**
```
src/
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   └── register/
│   ├── (dashboard)/
│   │   ├── layout.tsx
│   │   ├── page.tsx (dashboard home)
│   │   ├── appointments/
│   │   ├── customers/
│   │   └── settings/
│   └── api/ (route handlers)
├── components/
│   ├── ui/ (shadcn)
│   ├── appointments/
│   │   ├── appointment-card.tsx
│   │   ├── appointment-form.tsx
│   │   └── calendar.tsx
│   └── layout/
│       ├── sidebar.tsx
│       └── header.tsx
├── lib/
│   ├── api-client.ts
│   ├── supabase.ts
│   └── utils.ts
└── hooks/
    ├── use-appointments.ts
    └── use-auth.ts
```

**Checklist:**
- [ ] Next.js 14 instalado
- [ ] Tailwind CSS configurado
- [ ] ShadcnUI components instalados
- [ ] Estrutura de pastas criada
- [ ] Projeto roda sem erros

---

### Terça (13 Jan) - API Client & Auth

#### Implementar API Client

**Criar:** `src/lib/api-client.ts`
```typescript
import { useAuth } from '@/hooks/use-auth';

class ApiClient {
  private baseUrl = process.env.NEXT_PUBLIC_API_URL;
  
  async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const token = localStorage.getItem('access_token');
    
    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
        ...options?.headers,
      },
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message);
    }
    
    return response.json();
  }
  
  // Appointments
  appointments = {
    list: (filters?: AppointmentFilters) => 
      this.request<PagedResult<Appointment>>('/appointments', {
        method: 'GET',
      }),
    create: (data: CreateAppointmentDto) => 
      this.request<Appointment>('/appointments', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    // ... outros métodos
  };
}

export const apiClient = new ApiClient();
```

**React Query Hooks:**
```typescript
// src/hooks/use-appointments.ts
export function useAppointments(filters?: AppointmentFilters) {
  return useQuery({
    queryKey: ['appointments', filters],
    queryFn: () => apiClient.appointments.list(filters),
  });
}

export function useCreateAppointment() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: apiClient.appointments.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      toast.success('Agendamento criado! ✅');
    },
  });
}
```

**Checklist:**
- [ ] API Client implementado
- [ ] React Query configurado
- [ ] Auth hooks implementados
- [ ] Token management funcionando

---

### Quarta (14 Jan) - Dashboard Layout

#### Implementar Layout Base

**Criar:** `src/app/(dashboard)/layout.tsx`
```typescript
export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <Sidebar />
      
      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        <Header />
        <main className="flex-1 overflow-y-auto p-6 bg-gray-50">
          {children}
        </main>
      </div>
    </div>
  );
}
```

**Components:**
- [ ] `Sidebar.tsx` (navegação + logo)
- [ ] `Header.tsx` (search + user menu + command palette trigger)
- [ ] `CommandPalette.tsx` (Cmd+K)

**Checklist:**
- [ ] Layout responsivo (mobile-first)
- [ ] Sidebar com navegação
- [ ] Header com actions
- [ ] Dark mode toggle (opcional)

---

### Quinta (15 Jan) - Calendar Component ⭐

#### Implementar Calendar (Core UX)

**Opção 1: FullCalendar**
```bash
npm install @fullcalendar/react @fullcalendar/daygrid @fullcalendar/timegrid @fullcalendar/interaction
```

**Opção 2: Custom com DnD Kit** (recomendado para controle total)
```bash
npm install @dnd-kit/core @dnd-kit/sortable
```

**Criar:** `src/components/appointments/calendar.tsx`
```typescript
export function AppointmentCalendar() {
  const { data: appointments } = useAppointments({
    from: startOfWeek(new Date()),
    to: endOfWeek(new Date()),
  });
  
  return (
    <FullCalendar
      plugins={[dayGridPlugin, timeGridPlugin, interactionPlugin]}
      initialView="timeGridWeek"
      events={appointments?.data.map(apt => ({
        id: apt.id,
        title: apt.title,
        start: apt.scheduled_at,
        end: apt.ends_at,
        color: getStatusColor(apt.status),
      }))}
      editable
      eventDrop={handleEventDrop}
      dateClick={handleDateClick}
      slotDuration="00:30:00"
      businessHours={getBusinessHours()}
    />
  );
}
```

**Features:**
- [ ] Visualização de appointments
- [ ] Drag & Drop para reagendar
- [ ] Click em slot para criar appointment
- [ ] Color-coding por status
- [ ] Tooltip com detalhes

**Checklist:**
- [ ] Calendar renderizando
- [ ] Appointments visíveis
- [ ] Drag & Drop funcionando
- [ ] Create appointment on click

---

### Sexta (16 Jan) - Appointments CRUD

#### Implementar Forms & Dialogs

**Criar:** `src/components/appointments/appointment-form.tsx`
```typescript
const appointmentSchema = z.object({
  customer_id: z.string().uuid(),
  resource_id: z.string().uuid(),
  scheduled_at: z.date(),
  duration_minutes: z.number().min(15),
  title: z.string().min(3),
});

export function AppointmentForm({ appointment, onSuccess }: AppointmentFormProps) {
  const form = useForm<AppointmentFormData>({
    resolver: zodResolver(appointmentSchema),
    defaultValues: appointment,
  });
  
  const createMutation = useCreateAppointment();
  
  const onSubmit = async (data: AppointmentFormData) => {
    await createMutation.mutateAsync(data);
    onSuccess?.();
  };
  
  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)}>
        <FormField name="customer_id" label="Cliente" component={CustomerSelect} />
        <FormField name="scheduled_at" label="Data/Hora" component={DateTimePicker} />
        <FormField name="duration_minutes" label="Duração" component={DurationSelect} />
        <FormField name="title" label="Título" component={Input} />
        
        <Button type="submit" disabled={createMutation.isPending}>
          {createMutation.isPending ? 'Criando...' : 'Criar Agendamento'}
        </Button>
      </form>
    </Form>
  );
}
```

**CRUD Pages:**
- [ ] `/appointments` (list view com calendar)
- [ ] `/appointments/[id]` (detail view)
- [ ] Dialog para criar appointment
- [ ] Dialog para editar appointment

**Checklist:**
- [ ] Form validation com Zod
- [ ] Optimistic updates
- [ ] Error handling com toast
- [ ] Loading states

---

### Sábado-Domingo (17-18 Jan) - Customers & Deploy Frontend

#### Implementar Customers Module

**Pages:**
- [ ] `/customers` (list com search)
- [ ] `/customers/[id]` (detail + appointment history)
- [ ] Dialog para criar customer

**Features:**
- [ ] Search bar (fuzzy search)
- [ ] Filter por tags
- [ ] Pagination
- [ ] Export CSV

**Deploy no Vercel:**
```bash
# Instalar Vercel CLI
npm i -g vercel

# Deploy
vercel

# Configurar env vars
vercel env add NEXT_PUBLIC_API_URL
vercel env add NEXT_PUBLIC_SUPABASE_URL
vercel env add NEXT_PUBLIC_SUPABASE_ANON_KEY

# Deploy production
vercel --prod
```

**Checklist:**
- [ ] Customers CRUD funcionando
- [ ] Frontend deployado no Vercel
- [ ] Env vars configuradas
- [ ] DNS configurado (opcional)

---

## 📅 Semana 3 (19-25 Jan): Onboarding, Polish & Launch

### 🎯 Objetivos da Semana
- ✅ WhatsApp onboarding funcionando
- ✅ UX premium (micro-interações)
- ✅ Testes E2E passando
- ✅ MVP em produção

---

### Segunda (19 Jan) - WhatsApp Flow (n8n)

#### Setup n8n

**Opção 1: Cloud** (mais rápido)
```
- Criar conta em https://n8n.io
- Upgrade para plano pago ($20/mês)
```

**Opção 2: Self-hosted** (mais controle)
```bash
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n
```

#### Configurar Evolution API

```bash
# Clonar repo
git clone https://github.com/EvolutionAPI/evolution-api
cd evolution-api

# Configurar .env
cp .env.example .env
# Editar DATABASE_URL, JWT_SECRET, etc

# Rodar com Docker
docker-compose up -d

# Criar instância
curl -X POST http://localhost:8080/instance/create \
  -H "apikey: YOUR_API_KEY" \
  -d '{"instanceName": "astrafuture-prod"}'
```

#### Implementar Workflow n8n

**Importar workflow:**
- Copiar JSON de `workflows/whatsapp-onboarding.md`
- Importar no n8n
- Configurar credentials:
  - Evolution API (webhook URL)
  - AstraFuture API (API key)
  - OpenAI (API key para classificação)

**Testar flow:**
1. Enviar "Oi" para WhatsApp da instância
2. Seguir fluxo completo
3. Verificar se conta foi criada
4. Testar Magic Link

**Checklist:**
- [ ] n8n configurado
- [ ] Evolution API rodando
- [ ] Workflow importado
- [ ] Fluxo completo testado
- [ ] Magic Link funcionando

---

### Terça (20 Jan) - UX Premium (Micro-interações)

#### Implementar Optimistic Updates

```typescript
// Antes (sem optimistic update)
const deleteMutation = useDeleteAppointment();

// Depois (com optimistic update)
const deleteMutation = useMutation({
  mutationFn: apiClient.appointments.delete,
  onMutate: async (appointmentId) => {
    // Cancelar queries em andamento
    await queryClient.cancelQueries({ queryKey: ['appointments'] });
    
    // Snapshot do estado anterior
    const previousAppointments = queryClient.getQueryData(['appointments']);
    
    // Atualizar cache otimisticamente
    queryClient.setQueryData(['appointments'], (old: any) => ({
      ...old,
      data: old.data.filter((apt: any) => apt.id !== appointmentId),
    }));
    
    return { previousAppointments };
  },
  onError: (err, variables, context) => {
    // Reverter em caso de erro
    queryClient.setQueryData(['appointments'], context?.previousAppointments);
    toast.error('Erro ao deletar. Tente novamente.');
  },
  onSuccess: () => {
    toast.success('Agendamento cancelado! ✅');
  },
});
```

#### Adicionar Animações

```typescript
// Appointment Card com hover effect
<motion.div
  whileHover={{ y: -4, boxShadow: '0 12px 24px rgba(0,0,0,0.15)' }}
  whileTap={{ scale: 0.98 }}
  transition={{ duration: 0.2 }}
>
  <AppointmentCard />
</motion.div>

// List com stagger animation
<motion.ul
  variants={{
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: { staggerChildren: 0.05 }
    }
  }}
  initial="hidden"
  animate="show"
>
  {appointments.map(apt => (
    <motion.li
      key={apt.id}
      variants={{
        hidden: { opacity: 0, x: -20 },
        show: { opacity: 1, x: 0 }
      }}
    >
      <AppointmentCard appointment={apt} />
    </motion.li>
  ))}
</motion.ul>
```

**Checklist:**
- [ ] Optimistic updates em 5+ ações
- [ ] Hover states suaves
- [ ] Loading skeletons (não spinners)
- [ ] Toast notifications consistentes
- [ ] Transitions entre páginas

---

### Quarta (21 Jan) - Command Palette & Search

#### Implementar Cmd+K

```typescript
// src/components/command-palette.tsx
export function CommandPalette() {
  const [open, setOpen] = useState(false);
  const router = useRouter();
  
  useEffect(() => {
    const down = (e: KeyboardEvent) => {
      if (e.key === 'k' && (e.metaKey || e.ctrlKey)) {
        e.preventDefault();
        setOpen(true);
      }
    };
    
    document.addEventListener('keydown', down);
    return () => document.removeEventListener('keydown', down);
  }, []);
  
  return (
    <CommandDialog open={open} onOpenChange={setOpen}>
      <CommandInput placeholder="O que você precisa?" />
      <CommandList>
        <CommandGroup heading="Ações Rápidas">
          <CommandItem onSelect={() => router.push('/appointments/new')}>
            <CalendarPlus /> Novo Agendamento
          </CommandItem>
          <CommandItem onSelect={() => router.push('/customers/new')}>
            <UserPlus /> Adicionar Cliente
          </CommandItem>
        </CommandGroup>
        
        <CommandGroup heading="Buscar">
          <CommandItem>
            <Search /> Buscar clientes...
          </CommandItem>
        </CommandGroup>
      </CommandList>
    </CommandDialog>
  );
}
```

**Features:**
- [ ] Atalhos de teclado (Cmd+K abre, Esc fecha)
- [ ] Busca em appointments, customers
- [ ] Ações rápidas
- [ ] Navegação por teclado (↑↓ Enter)

**Checklist:**
- [ ] Command Palette funcionando
- [ ] Search com fuzzy matching
- [ ] Keyboard navigation
- [ ] Recent searches

---

### Quinta (22 Jan) - Testes E2E (Playwright)

#### Setup Playwright

```bash
npm init playwright@latest

# Instalar browsers
npx playwright install
```

**Criar testes:**
```typescript
// tests/appointments.spec.ts
test.describe('Appointments', () => {
  test('should create new appointment', async ({ page }) => {
    await page.goto('/appointments');
    
    // Click em "Novo Agendamento"
    await page.click('[data-testid="new-appointment"]');
    
    // Preencher form
    await page.fill('[name="customer_id"]', 'João Silva');
    await page.fill('[name="scheduled_at"]', '2026-01-25T14:00');
    await page.fill('[name="title"]', 'Consulta Inicial');
    
    // Submit
    await page.click('[type="submit"]');
    
    // Verificar toast de sucesso
    await expect(page.locator('.toast')).toContainText('Agendamento criado');
    
    // Verificar que appointment aparece no calendar
    await expect(page.locator('[data-appointment-id]')).toBeVisible();
  });
  
  test('should reschedule via drag and drop', async ({ page }) => {
    await page.goto('/appointments');
    
    // Drag appointment para outro horário
    const appointment = page.locator('[data-appointment-id="123"]');
    await appointment.dragTo(page.locator('[data-slot="2026-01-25T15:00"]'));
    
    // Verificar confirmação
    await expect(page.locator('.toast')).toContainText('Reagendado');
  });
});
```

**Tests a criar:**
- [ ] Auth flow (login, register)
- [ ] Appointments CRUD
- [ ] Customers CRUD
- [ ] Calendar interactions
- [ ] Command Palette

**Checklist:**
- [ ] 15+ testes E2E
- [ ] Todos os fluxos críticos cobertos
- [ ] CI rodando testes automaticamente

---

### Sexta (23 Jan) - Performance & Observability

#### Performance Optimization

**Lighthouse Audit:**
```bash
npm install -g lighthouse
lighthouse https://app.astrafuture.app --view
```

**Targets:**
- [ ] Performance > 90
- [ ] Accessibility > 95
- [ ] Best Practices > 90
- [ ] SEO > 90

**Otimizações:**
- [ ] Code splitting por rota
- [ ] Image optimization (next/image)
- [ ] Prefetching de links críticos
- [ ] Bundle analysis (`npm run build -- --analyze`)

#### Setup Monitoring

**Sentry (Error Tracking):**
```bash
npm install @sentry/nextjs
npx @sentry/wizard -i nextjs
```

**Vercel Analytics:**
```bash
npm install @vercel/analytics
```

**Configurar:**
```typescript
// app/layout.tsx
import { Analytics } from '@vercel/analytics/react';
import * as Sentry from '@sentry/nextjs';

Sentry.init({
  dsn: process.env.NEXT_PUBLIC_SENTRY_DSN,
  tracesSampleRate: 0.1,
});

export default function RootLayout({ children }) {
  return (
    <html>
      <body>
        {children}
        <Analytics />
      </body>
    </html>
  );
}
```

**Checklist:**
- [ ] Lighthouse score > 90
- [ ] Sentry configurado
- [ ] Vercel Analytics ativo
- [ ] Error boundaries implementados

---

### Sábado-Domingo (24-25 Jan) - Launch Prep & Documentation

#### Pre-Launch Checklist

**Segurança:**
- [ ] Rate limiting configurado (Redis)
- [ ] CORS configurado corretamente
- [ ] Secrets não estão no código
- [ ] RLS policies testadas
- [ ] SQL injection prevention (prepared statements)

**Performance:**
- [ ] Database indexes verificados
- [ ] Redis caching ativo
- [ ] CDN configurado (Cloudflare)
- [ ] Gzip/Brotli compression ativo

**UX:**
- [ ] Loading states em todas as actions
- [ ] Error messages úteis
- [ ] Empty states com CTAs
- [ ] Mobile responsive (testar em device real)
- [ ] Dark mode (opcional)

**Docs:**
- [ ] README atualizado
- [ ] API docs (Swagger) publicadas
- [ ] Onboarding guide para novos users
- [ ] Troubleshooting guide

#### Launch Day (25 Jan)

**Manhã:**
1. [ ] Rodar todos os testes (unit, integration, e2e)
2. [ ] Build de produção sem warnings
3. [ ] Deploy backend (Fly.io)
4. [ ] Deploy frontend (Vercel)
5. [ ] Smoke tests em produção

**Tarde:**
1. [ ] Criar tenant de demonstração
2. [ ] Testar fluxo completo end-to-end
3. [ ] Monitorar logs por 2h
4. [ ] Verificar métricas (Sentry, Analytics)

**Noite:**
1. [ ] Anunciar launch 🎉
2. [ ] Compartilhar link: https://app.astrafuture.app
3. [ ] Monitorar primeiros usuários
4. [ ] Responder feedback

---

## 🎯 Métricas de Sucesso (End of Week 3)

### Backend
- ✅ 21+ endpoints funcionando
- ✅ Response time P95 < 300ms
- ✅ Test coverage > 80%
- ✅ Zero erros 5xx em produção

### Frontend
- ✅ 5+ páginas implementadas
- ✅ Lighthouse score > 90
- ✅ Mobile responsive (100%)
- ✅ 15+ testes E2E passando

### UX
- ✅ Criar appointment em < 3 cliques
- ✅ Onboarding via WhatsApp < 3 min
- ✅ Zero formulários com > 5 campos
- ✅ Todas as ações com feedback visual

### Business
- ✅ 1+ tenant real usando
- ✅ 10+ appointments criados
- ✅ WhatsApp onboarding testado
- ✅ Magic Link funcionando

---

## 📝 Comandos Úteis (Copy-Paste Ready)

### Backend
```bash
# Build
dotnet build

# Run
dotnet run --project AstraFuture.Api

# Testes
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration

# Deploy
flyctl deploy
```

### Frontend
```bash
# Dev
npm run dev

# Build
npm run build
npm start

# Testes
npm run test
npm run test:e2e

# Deploy
vercel --prod
```

### Database
```bash
# Migrations
supabase db push
supabase db pull

# Seed
psql -U postgres -d astrafuture < database/seeds/001_demo_tenant.sql

# Backup
pg_dump -U postgres astrafuture > backup.sql
```

---

## 🚨 Red Flags - Pare e Resolva Imediatamente

- ❌ Testes falhando há > 1 dia
- ❌ Build quebrado por > 2h
- ❌ Erro 5xx em produção
- ❌ RLS vazando dados entre tenants
- ❌ Endpoint com > 1s de latência
- ❌ Lighthouse score < 70
- ❌ Coverage < 70%

---

## 💪 Você Consegue!

**Lembre-se:**
- ✅ MVP = Minimum **Viable** Product (não precisa ser perfeito)
- ✅ Priorize funcionalidade sobre perfeccionismo
- ✅ Deploy cedo, iterate rápido
- ✅ Monitore métricas, não sentimentos
- ✅ Automatize tudo que puder

**3 semanas é apertado, mas factível!** 🚀

Bora codar! 💻
