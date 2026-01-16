# 🎯 Próximos Passos - Semana 2

**Status Atual:** Dia 5 Completo - 70% do MVP  
**Próximo:** Dia 6 - Criar Appointment + Validação  
**Data:** 16 Janeiro 2026

---

## 📋 Dia 6 (22 Janeiro) - Criar Appointment

### 🎯 Objetivo
Implementar formulário completo de criação e edição de agendamentos.

### Manhã (4h)

#### 1. Dialog Component (1h)
```typescript
// frontend/src/components/ui/dialog.tsx
- Criar componente Dialog/Modal reutilizável
- Overlay com backdrop
- Animações de entrada/saída
- Fechar com ESC ou clique fora
```

#### 2. Appointment Form (2h)
```typescript
// frontend/src/components/appointments/appointment-form.tsx
- Formulário com React Hook Form
- Campos:
  - Cliente (select com busca)
  - Data (date picker)
  - Hora início (time picker)
  - Hora fim (time picker)
  - Notas (textarea)
  - Status (select)
```

#### 3. Validação com Zod (1h)
```typescript
// frontend/src/lib/validations/appointment.ts
export const appointmentSchema = z.object({
  customerId: z.string().uuid(),
  startTime: z.string().datetime(),
  endTime: z.string().datetime(),
  notes: z.string().optional(),
  status: z.enum(['scheduled', 'confirmed', 'completed', 'cancelled', 'no_show'])
})
  .refine(data => new Date(data.endTime) > new Date(data.startTime), {
    message: "Hora de fim deve ser após hora de início"
  })
```

### Tarde (4h)

#### 4. Integração com API (1h)
```typescript
// Conectar form com useCreateAppointment()
- Submit handler
- Loading states
- Error handling
- Success redirect
```

#### 5. Edição de Appointments (2h)
```typescript
// frontend/src/components/appointments/edit-appointment-dialog.tsx
- Reutilizar appointment-form
- Preencher com dados existentes
- useUpdateAppointment() hook
- Atualizar lista após edição
```

#### 6. Toast Notifications (0.5h)
```typescript
// Melhorar feedback visual
- Success toast com ícone
- Error toast com detalhes
- Loading toast durante operações
```

#### 7. Testes E2E (0.5h)
```
Testar fluxo completo:
1. Login
2. Criar appointment
3. Verificar na lista
4. Editar appointment
5. Verificar mudanças
6. Deletar appointment
7. Verificar exclusão
```

---

## 🛠️ Componentes a Criar

### UI Components

1. **Dialog** (`components/ui/dialog.tsx`)
   ```typescript
   interface DialogProps {
     open: boolean
     onOpenChange: (open: boolean) => void
     title: string
     description?: string
     children: React.ReactNode
   }
   ```

2. **Select** (`components/ui/select.tsx`)
   ```typescript
   - Native select estilizado
   - Suporte a busca (opcional)
   - Multi-select (futuro)
   ```

3. **Textarea** (`components/ui/textarea.tsx`)
   ```typescript
   - Similar ao Input
   - Auto-resize (opcional)
   ```

4. **DatePicker** (`components/ui/date-picker.tsx`)
   ```typescript
   - Usando date-fns
   - Formato pt-BR
   - Pode usar biblioteca react-datepicker
   ```

5. **TimePicker** (`components/ui/time-picker.tsx`)
   ```typescript
   - Input com máscara HH:mm
   - Validação de horário
   ```

### Business Components

6. **AppointmentDialog** (`components/appointments/appointment-dialog.tsx`)
   ```typescript
   interface AppointmentDialogProps {
     open: boolean
     onOpenChange: (open: boolean) => void
     appointment?: Appointment // undefined = create, definido = edit
     onSuccess?: () => void
   }
   ```

7. **CustomerSelect** (`components/customers/customer-select.tsx`)
   ```typescript
   - Busca de clientes
   - Opção de criar novo inline
   - Loading states
   ```

---

## 📦 Dependências Adicionais

Instalar no frontend:

```bash
cd frontend

# Form handling
npm install react-hook-form @hookform/resolvers zod

# Date/Time pickers
npm install react-datepicker date-fns
npm install -D @types/react-datepicker

# Dialogs/Modals
npm install @radix-ui/react-dialog

# Select com busca (opcional)
npm install @radix-ui/react-select
```

---

## 🎨 Fluxo de UX

### Criar Appointment

```
1. Usuário clica em "Novo Agendamento"
   ↓
2. Dialog abre com formulário vazio
   ↓
3. Usuário seleciona cliente
   ↓
4. Usuário seleciona data
   ↓
5. Usuário seleciona horários
   ↓
6. Usuário adiciona notas (opcional)
   ↓
7. Clica em "Criar"
   ↓
8. Loading spinner aparece
   ↓
9. Sucesso:
   - Toast "Agendamento criado!"
   - Dialog fecha
   - Lista atualiza
   OU
10. Erro:
   - Toast com mensagem de erro
   - Form permanece aberto
   - Campos com erro destacados
```

### Editar Appointment

```
1. Usuário clica no ícone de editar no card
   ↓
2. Dialog abre com dados preenchidos
   ↓
3. Usuário modifica campos desejados
   ↓
4. Clica em "Salvar"
   ↓
5. Loading + atualização da lista
```

---

## 🧪 Casos de Teste

### Validação

- [ ] Não permite criar sem cliente
- [ ] Não permite criar sem data
- [ ] Não permite hora fim antes de hora início
- [ ] Valida formato de data
- [ ] Valida formato de hora
- [ ] Limita tamanho das notas

### Integração

- [ ] Create via API funciona
- [ ] Update via API funciona
- [ ] Lista atualiza após create
- [ ] Lista atualiza após update
- [ ] Dialog fecha após sucesso
- [ ] Toast aparece após sucesso/erro

### UX

- [ ] Loading states aparecem
- [ ] Erros são exibidos claramente
- [ ] Dialog pode ser fechado com ESC
- [ ] Dialog pode ser fechado clicando fora
- [ ] Formulário é resetado ao fechar

---

## 📝 Arquivos a Modificar/Criar

### Novos Arquivos
```
frontend/src/
├── components/
│   ├── ui/
│   │   ├── dialog.tsx              ⭐ NOVO
│   │   ├── select.tsx              ⭐ NOVO
│   │   ├── textarea.tsx            ⭐ NOVO
│   │   ├── date-picker.tsx         ⭐ NOVO
│   │   └── time-picker.tsx         ⭐ NOVO
│   ├── appointments/
│   │   ├── appointment-dialog.tsx  ⭐ NOVO
│   │   └── appointment-form.tsx    ⭐ NOVO
│   └── customers/
│       └── customer-select.tsx     ⭐ NOVO
└── lib/
    └── validations/
        └── appointment.ts          ⭐ NOVO
```

### Arquivos a Modificar
```
frontend/src/
└── app/
    └── dashboard/
        └── appointments/
            └── page.tsx            🔄 ATUALIZAR
```

---

## 🚀 Comandos Úteis

### Desenvolvimento
```bash
# Terminal 1 - Backend
cd backend-src/AstraFuture.Api
dotnet watch run

# Terminal 2 - Frontend
cd frontend
npm run dev

# Terminal 3 - Logs/Testes
cd frontend
npm run lint
```

### Debug
```bash
# Limpar cache Next.js
rm -rf .next

# Reinstalar dependências
rm -rf node_modules package-lock.json
npm install

# Build de teste
npm run build
```

---

## 📊 Métricas de Sucesso do Dia 6

Ao final do dia, deve ser possível:

- [ ] Abrir dialog de criação
- [ ] Preencher formulário completo
- [ ] Ver validações funcionando
- [ ] Criar appointment com sucesso
- [ ] Ver novo appointment na lista
- [ ] Editar appointment existente
- [ ] Ver mudanças refletidas na lista
- [ ] Todas as operações com feedback visual

**Meta:** CRUD 100% completo e funcional! ✅

---

## 🔮 Dias 7-10 (Preview)

### Dia 7 (23 Jan) - Deploy
- Deploy backend no Fly.io
- Deploy frontend no Vercel
- Configurar variáveis de ambiente
- Smoke tests em produção

### Dia 8 (24 Jan) - Polish
- Correção de bugs
- Melhorias de UX
- Responsividade mobile
- Testes completos

### Dias 9-10 (25-26 Jan) - Launch
- Buffer para imprevistos
- Preparação de demo
- Documentação final
- 🚀 LAUNCH!

---

## 💡 Dicas

### Performance
- Use React.memo() para components pesados
- Debounce em buscas (customer select)
- Validação assíncrona se necessário

### UX
- Loading states SEMPRE
- Feedback visual imediato
- Mensagens de erro claras
- Confirmação antes de ações destrutivas

### Código
- Mantenha components pequenos (<200 linhas)
- Extraia lógica complexa para hooks
- Reutilize components UI
- Mantenha types atualizados

---

## 📚 Recursos

### Documentação
- [React Hook Form](https://react-hook-form.com/)
- [Zod](https://zod.dev/)
- [Radix UI](https://www.radix-ui.com/)
- [date-fns](https://date-fns.org/)

### Exemplos
Veja as implementações existentes em:
- `components/ui/button.tsx` - Padrão de component UI
- `hooks/use-appointments.ts` - Padrão de custom hook
- `app/dashboard/appointments/page.tsx` - Padrão de página

---

## ✅ Checklist Antes de Começar

- [ ] Backend rodando sem erros
- [ ] Frontend rodando sem erros
- [ ] Dia 5 completamente testado
- [ ] Documentação do Dia 5 lida
- [ ] Lista de tarefas do Dia 6 clara
- [ ] Café preparado ☕

---

**Boa sorte no Dia 6! Você consegue! 💪**

---

**Última atualização:** 16 Janeiro 2026
