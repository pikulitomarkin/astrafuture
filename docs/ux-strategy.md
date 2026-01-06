# UX Strategy Premium - AstraFuture

## Filosofia de Design

> "Software premium não é sobre mais features, é sobre **zero fricção** entre intenção e ação."

---

## Princípios Core

### 1. **Invisibilidade Inteligente**
O melhor design é aquele que o usuário não percebe. A interface deve antecipar necessidades antes que o usuário precise pensar.

**Anti-patterns a evitar:**
- ❌ Formulários com 10+ campos
- ❌ Múltiplos cliques para ações simples
- ❌ Confirmações excessivas ("Tem certeza?")
- ❌ Carregamento sem feedback visual

**Padrões premium:**
- ✅ Criar appointment em 2 cliques
- ✅ Auto-save em tempo real (sem botão "Salvar")
- ✅ Atalhos de teclado para power users
- ✅ Skeleton loaders + optimistic updates

---

### 2. **Performance Percebida > Performance Real**

Usuários julgam velocidade por **sensação**, não por milissegundos reais.

**Técnicas:**

#### Optimistic Updates
```tsx
// Antes (ruim - usuário espera resposta)
const handleComplete = async () => {
  setLoading(true);
  await api.appointments.complete(id);
  setLoading(false);
  refetch();
};

// Depois (premium - atualiza instantaneamente)
const handleComplete = async () => {
  // Atualiza UI imediatamente
  queryClient.setQueryData(['appointment', id], (old) => ({
    ...old,
    status: 'completed'
  }));
  
  // Envia ao backend em background
  try {
    await api.appointments.complete(id);
  } catch (error) {
    // Se falhar, reverte UI
    queryClient.invalidateQueries(['appointment', id]);
    showToast('Erro ao completar. Tente novamente.');
  }
};
```

#### Skeleton Loaders (não spinners)
```tsx
// ❌ Ruim
{isLoading && <Spinner />}

// ✅ Premium
{isLoading ? (
  <div className="space-y-4">
    <Skeleton className="h-20 w-full" />
    <Skeleton className="h-20 w-full" />
    <Skeleton className="h-20 w-full" />
  </div>
) : (
  <AppointmentList data={appointments} />
)}
```

#### Prefetching
```tsx
// Quando hover em appointment card, prefetch detalhes
<AppointmentCard
  onMouseEnter={() => {
    queryClient.prefetchQuery(
      ['appointment', appointment.id],
      () => api.appointments.get(appointment.id)
    );
  }}
/>
```

---

### 3. **Micro-interações Deliciosas**

Pequenos detalhes que transformam funcional em **delightful**.

#### Hover States com Depth
```css
.appointment-card {
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.appointment-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
}
```

#### Status Change Animation
```tsx
<AnimatePresence mode="wait">
  <motion.div
    key={appointment.status}
    initial={{ opacity: 0, scale: 0.9 }}
    animate={{ opacity: 1, scale: 1 }}
    exit={{ opacity: 0, scale: 0.9 }}
    transition={{ duration: 0.2 }}
  >
    <Badge status={appointment.status} />
  </motion.div>
</AnimatePresence>
```

#### Success Feedback
```tsx
const handleSave = async () => {
  await api.save(data);
  
  // Micro-celebration
  confetti({
    particleCount: 50,
    spread: 60,
    origin: { y: 0.8 }
  });
  
  showToast('Salvo com sucesso! ✨', { 
    icon: '✅',
    duration: 2000 
  });
};
```

---

### 4. **Zero Formulários Tradicionais**

Formulários são o **maior killer de conversão**. Substituir por interações naturais.

#### Inline Editing
```tsx
// ❌ Modal com formulário
<Modal>
  <Form>
    <Input label="Título" />
    <Button>Salvar</Button>
  </Form>
</Modal>

// ✅ Edição inline (como Notion)
<div
  contentEditable
  onBlur={(e) => {
    if (e.target.innerText !== title) {
      updateTitle(e.target.innerText);
    }
  }}
>
  {title}
</div>
```

#### Smart Defaults
```tsx
// Ao criar appointment, preencher automaticamente:
const smartDefaults = {
  scheduled_at: nextAvailableSlot(), // Próximo horário livre
  duration_minutes: tenant.business_rules.min_duration_minutes,
  resource_id: user.default_resource_id,
  location: resource.default_location
};
```

#### Command Palette (Cmd+K)
```tsx
<CommandPalette>
  <Command.Input placeholder="O que você precisa?" />
  <Command.List>
    <Command.Group heading="Ações Rápidas">
      <Command.Item onSelect={createAppointment}>
        <CalendarIcon /> Novo Agendamento
      </Command.Item>
      <Command.Item onSelect={addCustomer}>
        <UserIcon /> Adicionar Cliente
      </Command.Item>
    </Command.Group>
    
    <Command.Group heading="Buscar">
      {/* Busca em tempo real */}
    </Command.Group>
  </Command.List>
</CommandPalette>
```

---

### 5. **Mobile-First, Desktop-Optimized**

Maioria dos usuários acessa via mobile, mas power users preferem desktop.

#### Responsive Patterns
```tsx
// Mobile: Bottom Sheet para ações
<Sheet>
  <SheetTrigger asChild>
    <Button>Opções</Button>
  </SheetTrigger>
  <SheetContent side="bottom">
    <ActionMenu />
  </SheetContent>
</Sheet>

// Desktop: Context Menu (right-click)
<ContextMenu>
  <ContextMenuTrigger>
    <AppointmentCard />
  </ContextMenuTrigger>
  <ContextMenuContent>
    <ContextMenuItem>Reagendar</ContextMenuItem>
    <ContextMenuItem>Cancelar</ContextMenuItem>
  </ContextMenuContent>
</ContextMenu>
```

#### Gestures (Mobile)
```tsx
// Swipe para completar appointment
<motion.div
  drag="x"
  dragConstraints={{ left: 0, right: 100 }}
  onDragEnd={(e, { offset }) => {
    if (offset.x > 80) {
      completeAppointment();
    }
  }}
>
  <AppointmentCard />
</motion.div>
```

---

## Componentes Premium

### 1. Calendar (Core Component)

**Requisitos:**
- ✅ Drag & Drop para reagendar
- ✅ Multi-view (dia, semana, mês)
- ✅ Color-coding por status
- ✅ Conflito visual (overlay vermelho)
- ✅ Click para criar appointment no horário

**Biblioteca:** [FullCalendar](https://fullcalendar.io/) ou custom com [DnD Kit](https://dndkit.com/)

```tsx
<Calendar
  view="week"
  events={appointments}
  onEventDrop={handleReschedule}
  onDateClick={handleCreateAppointment}
  slotDuration="00:30:00"
  businessHours={resource.working_hours}
  eventColor={(event) => getStatusColor(event.status)}
/>
```

---

### 2. Search (Instant, Fuzzy)

**UX:**
- Abrir com `Cmd+K` ou `/`
- Buscar em: customers, appointments, resources
- Highlight de match
- Navegação por teclado (↑↓ Enter)

```tsx
<Command>
  <Command.Input 
    placeholder="Buscar cliente, agendamento..."
    value={query}
    onValueChange={setQuery}
  />
  
  <Command.List>
    {/* Backend usa pg_trgm para fuzzy search */}
    {results.map(result => (
      <Command.Item
        key={result.id}
        onSelect={() => navigate(result.url)}
      >
        <ResultIcon type={result.type} />
        <span dangerouslySetInnerHTML={{ 
          __html: highlightMatch(result.title, query) 
        }} />
      </Command.Item>
    ))}
  </Command.List>
</Command>
```

**Backend (PostgreSQL):**
```sql
SELECT 
  id,
  full_name,
  similarity(full_name, 'maria') AS score
FROM customers
WHERE full_name % 'maria' -- pg_trgm operator
ORDER BY score DESC
LIMIT 10;
```

---

### 3. Notifications (Non-intrusive)

**Hierarquia:**
1. **Toast** (sucesso, info) - 3s, canto superior direito
2. **Alert** (warning) - Persistente até dismissar
3. **Modal** (erro crítico) - Bloqueia interação

```tsx
// Toast (sucesso)
toast.success('Agendamento criado! ✅', {
  description: 'Cliente receberá confirmação via WhatsApp',
  action: {
    label: 'Ver detalhes',
    onClick: () => navigate(`/appointments/${id}`)
  }
});

// Alert (warning)
<Alert variant="warning">
  <AlertTriangle />
  <AlertTitle>Conflito de horário</AlertTitle>
  <AlertDescription>
    Já existe um agendamento neste horário. 
    <Link to="/calendar">Ver conflitos</Link>
  </AlertDescription>
</Alert>

// Modal (erro crítico)
<AlertDialog open={hasPaymentError}>
  <AlertDialogContent>
    <AlertDialogHeader>
      <AlertDialogTitle>Erro no pagamento</AlertDialogTitle>
      <AlertDialogDescription>
        Não foi possível processar o pagamento. 
        Verifique os dados do cartão.
      </AlertDialogDescription>
    </AlertDialogHeader>
    <AlertDialogFooter>
      <AlertDialogAction>Tentar novamente</AlertDialogAction>
    </AlertDialogFooter>
  </AlertDialogContent>
</AlertDialog>
```

---

### 4. Empty States (Educativos)

**Nunca mostrar tela vazia sem contexto.**

```tsx
// ❌ Ruim
{appointments.length === 0 && <p>Sem agendamentos</p>}

// ✅ Premium
{appointments.length === 0 && (
  <EmptyState
    icon={<CalendarIcon className="w-16 h-16 text-muted" />}
    title="Nenhum agendamento ainda"
    description="Comece criando seu primeiro agendamento ou compartilhe seu link de agendamento com clientes."
    actions={[
      <Button onClick={createAppointment}>
        Criar Agendamento
      </Button>,
      <Button variant="outline" onClick={copyBookingLink}>
        Copiar Link de Agendamento
      </Button>
    ]}
  />
)}
```

---

### 5. Data Tables (Powerful, não chatos)

**Features:**
- ✅ Busca inline
- ✅ Filtros multi-seleção
- ✅ Ordenação por coluna
- ✅ Bulk actions (selecionar múltiplos)
- ✅ Export (CSV, PDF)
- ✅ Column visibility toggle

```tsx
<DataTable
  columns={[
    { 
      id: 'customer', 
      header: 'Cliente',
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <Avatar src={row.customer.avatar_url} />
          <span>{row.customer.full_name}</span>
        </div>
      ),
      enableSorting: true,
      enableFiltering: true
    },
    { 
      id: 'scheduled_at', 
      header: 'Data/Hora',
      cell: ({ row }) => formatDateTime(row.scheduled_at),
      enableSorting: true
    },
    {
      id: 'actions',
      cell: ({ row }) => (
        <DropdownMenu>
          <DropdownMenuTrigger>⋮</DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuItem>Editar</DropdownMenuItem>
            <DropdownMenuItem>Cancelar</DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      )
    }
  ]}
  data={appointments}
  onRowClick={(row) => navigate(`/appointments/${row.id}`)}
/>
```

---

## Design System

### Color Palette (Semantic)

```tsx
const colors = {
  // Status colors
  status: {
    scheduled: 'hsl(210, 100%, 50%)',   // Blue
    confirmed: 'hsl(150, 80%, 45%)',    // Green
    in_progress: 'hsl(45, 100%, 50%)',  // Yellow
    completed: 'hsl(150, 60%, 35%)',    // Dark Green
    cancelled: 'hsl(0, 70%, 50%)',      // Red
    no_show: 'hsl(0, 0%, 40%)'          // Gray
  },
  
  // Semantic colors
  primary: 'hsl(var(--primary))',       // Tenant customizable
  destructive: 'hsl(var(--destructive))',
  success: 'hsl(142, 71%, 45%)',
  warning: 'hsl(38, 92%, 50%)',
  info: 'hsl(210, 100%, 50%)'
};
```

### Typography (Legível, Hierárquica)

```css
/* Heading 1 (página) */
.h1 {
  font-size: 2.25rem; /* 36px */
  font-weight: 700;
  line-height: 2.5rem;
  letter-spacing: -0.02em;
}

/* Heading 2 (seção) */
.h2 {
  font-size: 1.875rem; /* 30px */
  font-weight: 600;
  line-height: 2.25rem;
  letter-spacing: -0.01em;
}

/* Body */
.body {
  font-size: 1rem; /* 16px */
  font-weight: 400;
  line-height: 1.5rem;
}

/* Caption */
.caption {
  font-size: 0.875rem; /* 14px */
  font-weight: 400;
  line-height: 1.25rem;
  color: hsl(var(--muted-foreground));
}
```

### Spacing (Sistema 4pt)

```tsx
const spacing = {
  0: '0px',
  1: '4px',
  2: '8px',
  3: '12px',
  4: '16px',
  5: '20px',
  6: '24px',
  8: '32px',
  10: '40px',
  12: '48px',
  16: '64px',
  20: '80px'
};
```

---

## Animações & Transições

### Timing Functions (Natural)

```css
/* Ease Out (padrão para entrada) */
transition: all 0.2s cubic-bezier(0, 0, 0.2, 1);

/* Ease In (para saída) */
transition: all 0.15s cubic-bezier(0.4, 0, 1, 1);

/* Bounce (para success feedback) */
transition: all 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
```

### Durações

| Ação | Duração | Uso |
|------|---------|-----|
| Hover | 150ms | Feedback instantâneo |
| Transition | 200ms | Mudança de estado |
| Modal Open | 300ms | Entrada de componente grande |
| Page Transition | 400ms | Navegação entre páginas |

### Exemplos

#### Fade In/Out
```tsx
<motion.div
  initial={{ opacity: 0 }}
  animate={{ opacity: 1 }}
  exit={{ opacity: 0 }}
  transition={{ duration: 0.2 }}
>
  {content}
</motion.div>
```

#### Slide Up (Mobile Bottom Sheet)
```tsx
<motion.div
  initial={{ y: '100%' }}
  animate={{ y: 0 }}
  exit={{ y: '100%' }}
  transition={{ type: 'spring', damping: 30, stiffness: 300 }}
>
  {sheet}
</motion.div>
```

#### Stagger Children (Lista)
```tsx
<motion.ul
  variants={{
    hidden: { opacity: 0 },
    show: {
      opacity: 1,
      transition: {
        staggerChildren: 0.05
      }
    }
  }}
  initial="hidden"
  animate="show"
>
  {items.map(item => (
    <motion.li
      key={item.id}
      variants={{
        hidden: { opacity: 0, x: -20 },
        show: { opacity: 1, x: 0 }
      }}
    >
      {item.name}
    </motion.li>
  ))}
</motion.ul>
```

---

## Acessibilidade (A11y)

### WCAG 2.1 Level AA Compliance

#### Contraste de Cores
```
Texto normal:  mínimo 4.5:1
Texto grande:  mínimo 3:1
UI components: mínimo 3:1
```

#### Keyboard Navigation
```tsx
// Todos os elementos interativos acessíveis via Tab
<Button tabIndex={0}>Acessível</Button>

// Atalhos customizados
useHotkeys('ctrl+k', () => openCommandPalette());
useHotkeys('ctrl+n', () => createAppointment());

// Focus trap em modals
<Dialog onOpenChange={(open) => {
  if (open) {
    focusTrap.activate();
  } else {
    focusTrap.deactivate();
  }
}}>
```

#### Screen Readers
```tsx
// ARIA labels
<button aria-label="Criar novo agendamento">
  <PlusIcon />
</button>

// Live regions para updates dinâmicos
<div aria-live="polite" aria-atomic="true">
  {successMessage}
</div>

// Skip links
<a href="#main-content" className="sr-only focus:not-sr-only">
  Pular para conteúdo principal
</a>
```

---

## Performance Budgets

| Métrica | Target | Medição |
|---------|--------|---------|
| **First Contentful Paint** | < 1.5s | Lighthouse |
| **Time to Interactive** | < 3s | Lighthouse |
| **Largest Contentful Paint** | < 2.5s | Lighthouse |
| **Cumulative Layout Shift** | < 0.1 | Lighthouse |
| **Bundle Size (initial)** | < 200KB | Bundlephobia |
| **API Response Time (P95)** | < 300ms | Backend monitoring |

### Técnicas de Otimização

#### Code Splitting
```tsx
// Lazy load de páginas
const AppointmentsPage = lazy(() => import('./pages/Appointments'));
const CustomersPage = lazy(() => import('./pages/Customers'));

<Suspense fallback={<PageSkeleton />}>
  <Routes>
    <Route path="/appointments" element={<AppointmentsPage />} />
    <Route path="/customers" element={<CustomersPage />} />
  </Routes>
</Suspense>
```

#### Image Optimization
```tsx
<Image
  src="/logo.png"
  alt="Clínica Psique"
  width={200}
  height={50}
  loading="lazy"
  placeholder="blur"
  blurDataURL="data:image/jpeg;base64,..."
/>
```

#### Virtual Scrolling (listas grandes)
```tsx
import { useVirtualizer } from '@tanstack/react-virtual';

const virtualizer = useVirtualizer({
  count: appointments.length,
  getScrollElement: () => parentRef.current,
  estimateSize: () => 80,
  overscan: 5
});

<div ref={parentRef} style={{ height: '600px', overflow: 'auto' }}>
  <div style={{ height: virtualizer.getTotalSize() }}>
    {virtualizer.getVirtualItems().map(virtualRow => (
      <div
        key={virtualRow.index}
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%',
          transform: `translateY(${virtualRow.start}px)`
        }}
      >
        <AppointmentCard appointment={appointments[virtualRow.index]} />
      </div>
    ))}
  </div>
</div>
```

---

## Dark Mode (Opcional, mas premium)

### Toggle Suave
```tsx
const [theme, setTheme] = useState<'light' | 'dark'>('light');

// Transição suave entre temas
<motion.div
  initial={false}
  animate={{ 
    backgroundColor: theme === 'dark' ? '#000' : '#fff',
    color: theme === 'dark' ? '#fff' : '#000'
  }}
  transition={{ duration: 0.3 }}
>
  {content}
</motion.div>

// Persistir preferência
useEffect(() => {
  localStorage.setItem('theme', theme);
}, [theme]);
```

### CSS Variables
```css
:root {
  --background: 0 0% 100%;
  --foreground: 240 10% 3.9%;
  --primary: 240 5.9% 10%;
}

.dark {
  --background: 240 10% 3.9%;
  --foreground: 0 0% 98%;
  --primary: 0 0% 98%;
}
```

---

## Checklist de Qualidade (QA)

Antes de cada release:

### Visual
- [ ] Todas as cores têm contraste adequado (WCAG AA)
- [ ] Hover states em todos os elementos interativos
- [ ] Loading states em todas as async actions
- [ ] Empty states em todas as listas
- [ ] Error states com mensagens úteis

### Funcional
- [ ] Formulários validam antes de enviar
- [ ] Erros de API são tratados com fallback
- [ ] Optimistic updates revertem em caso de erro
- [ ] Offline detection (toast: "Sem conexão")
- [ ] Rate limit handling (toast: "Tente novamente em X segundos")

### Performance
- [ ] Images otimizadas (WebP, lazy loading)
- [ ] Bundle size < 200KB (initial)
- [ ] Lighthouse score > 90
- [ ] Nenhum layout shift visível (CLS < 0.1)

### Mobile
- [ ] Touch targets > 44x44px
- [ ] Gestures funcionam (swipe, long-press)
- [ ] Bottom sheets para ações (não modals)
- [ ] Safe area (iOS notch)

### Acessibilidade
- [ ] Navegação completa por teclado
- [ ] Screen reader friendly (ARIA labels)
- [ ] Focus indicators visíveis
- [ ] Contraste de cores adequado

---

## Inspirações (Benchmarks)

Estudar UX de:
- **Linear** (project management) - Micro-interações, Command Palette
- **Notion** (workspace) - Inline editing, drag & drop
- **Cal.com** (scheduling) - Booking flow, calendar UX
- **Stripe Dashboard** - Data tables, charts, clarity
- **Vercel** - Deploy feedback, status updates

---

**Próximo:** [Clean Architecture Project Scaffold](../src/)
