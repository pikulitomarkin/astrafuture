# 🚀 Setup Supabase - Passo a Passo

**Tempo estimado:** 30 minutos  
**Pré-requisito:** Conta no Supabase (criar em https://supabase.com se não tiver)

---

## Passo 1: Criar Projeto Supabase

1. **Acesse:** https://supabase.com
2. **Login** com GitHub ou email
3. **Click:** "New Project"

### Configurações:
```
Name: astrafuture-prod
Database Password: [GERE SENHA FORTE - SALVE EM SEGURANÇA]
Region: South America (São Paulo)
Pricing Plan: Free (suficiente para MVP)
```

4. **Click:** "Create new project"
5. **Aguarde ~2 minutos** (provisioning do database)

---

## Passo 2: Salvar Credenciais

Após criação, vá em **Settings > API**:

### Copie e salve (vamos usar depois):

```bash
# PROJECT URL
SUPABASE_URL=https://xxxxxxxxxx.supabase.co

# ANON KEY (public)
SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

# SERVICE_ROLE KEY (secret - NUNCA commitar!)
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Crie arquivo local:
```bash
# Em d:\Astrafuture\
echo. > .env.local
```

**Adicione no `.env.local`:**
```env
SUPABASE_URL=https://sua-url.supabase.co
SUPABASE_ANON_KEY=sua-anon-key
SUPABASE_SERVICE_ROLE_KEY=sua-service-role-key
DATABASE_URL=postgresql://postgres:[SUA-SENHA]@db.sua-url.supabase.co:5432/postgres
```

⚠️ **IMPORTANTE:** `.env.local` já está no `.gitignore`, nunca faça commit!

---

## Passo 3: Executar Schema

### Opção A: Via SQL Editor (Recomendado)

1. No Supabase Dashboard, vá em **SQL Editor**
2. Click em **New Query**
3. Copie **TODO** o conteúdo de `d:\Astrafuture\database\schema.sql`
4. Cole no editor
5. Click em **RUN** (Ctrl+Enter)
6. Aguarde ~10 segundos
7. Verificar: "Success. No rows returned"

### Opção B: Via Supabase CLI (Alternativa)

```bash
# Instalar CLI (se não tiver)
npm install -g supabase

# Login
supabase login

# Link ao projeto
supabase link --project-ref sua-project-ref

# Push schema
supabase db push
```

---

## Passo 4: Verificar Tabelas Criadas

1. Vá em **Table Editor**
2. Deve ver estas tabelas:
   - ✅ `tenants`
   - ✅ `users`
   - ✅ `customers`
   - ✅ `resources`
   - ✅ `appointments`
   - ✅ `availability_rules`
   - ✅ `notifications`
   - ✅ `webhooks`
   - ✅ `audit_logs`

3. Click em `tenants` → deve estar vazia (ok)

---

## Passo 5: Inserir Seed Data (Tenant Demo)

### Via SQL Editor:

```sql
-- Criar tenant demo
INSERT INTO tenants (id, name, slug, tenant_type, subscription_tier, is_active, onboarding_completed_at)
VALUES (
  '00000000-0000-0000-0000-000000000001',
  'Clínica Psique Demo',
  'clinica-psique-demo',
  'psychology',
  'pro',
  true,
  NOW()
);

-- Criar user owner (vincular depois com Supabase Auth)
INSERT INTO users (id, tenant_id, auth_user_id, email, full_name, role, is_active, email_verified_at)
VALUES (
  '00000000-0000-0000-0000-000000000002',
  '00000000-0000-0000-0000-000000000001',
  '00000000-0000-0000-0000-000000000099', -- Temporário, será substituído no registro real
  'demo@clinicapsique.com',
  'Dr. Ana Silva',
  'owner',
  true,
  NOW()
);

-- Criar resource (profissional)
INSERT INTO resources (id, tenant_id, name, resource_type, is_active)
VALUES (
  '00000000-0000-0000-0000-000000000003',
  '00000000-0000-0000-0000-000000000001',
  'Dra. Ana Silva - Consultório 1',
  'professional',
  true
);

-- Criar customer de exemplo
INSERT INTO customers (id, tenant_id, name, email, phone, customer_type)
VALUES (
  '00000000-0000-0000-0000-000000000004',
  '00000000-0000-0000-0000-000000000001',
  'João Santos',
  'joao.santos@example.com',
  '+5511999999999',
  'individual'
);

-- Criar appointment de exemplo (amanhã às 14h)
INSERT INTO appointments (
  id,
  tenant_id,
  customer_id,
  resource_id,
  title,
  scheduled_at,
  ends_at,
  duration_minutes,
  status,
  appointment_type
)
VALUES (
  '00000000-0000-0000-0000-000000000005',
  '00000000-0000-0000-0000-000000000001',
  '00000000-0000-0000-0000-000000000004',
  '00000000-0000-0000-0000-000000000003',
  'Consulta Inicial - João Santos',
  (NOW() + INTERVAL '1 day')::DATE + TIME '14:00:00',
  (NOW() + INTERVAL '1 day')::DATE + TIME '15:00:00',
  60,
  'scheduled',
  'consultation'
);
```

**Execute** no SQL Editor → "Success"

---

## Passo 6: Testar RLS (Row-Level Security)

### No SQL Editor:

```sql
-- Simular contexto de tenant
SET LOCAL app.tenant_id = '00000000-0000-0000-0000-000000000001';

-- Deve retornar 1 appointment
SELECT * FROM appointments;

-- Resetar contexto
SET LOCAL app.tenant_id = '00000000-0000-0000-0000-000000000999';

-- Deve retornar 0 (RLS bloqueou)
SELECT * FROM appointments;
```

**Resultado esperado:**
- 1ª query: 1 row
- 2ª query: 0 rows

✅ **RLS está funcionando!**

---

## Passo 7: Configurar Autenticação Supabase

### No Dashboard:

1. Vá em **Authentication > Providers**
2. **Email** já está habilitado por padrão
3. Configurar **Site URL** e **Redirect URLs**:

```
Site URL: http://localhost:3000
Redirect URLs: 
  - http://localhost:3000/auth/callback
  - https://seu-app.vercel.app/auth/callback (depois)
```

4. **Disable Email Confirmations** (para MVP):
   - Settings > Auth > Email Auth > **Desmarcar** "Enable email confirmations"
   - Isso acelera testes (não precisa confirmar email)

---

## Passo 8: Testar Conexão via Postman (Opcional)

### Request:
```
GET https://sua-url.supabase.co/rest/v1/tenants?select=*
Headers:
  apikey: [SUA_ANON_KEY]
  Authorization: Bearer [SUA_ANON_KEY]
```

**Resultado esperado:** JSON com 1 tenant

---

## ✅ Checklist Final

Antes de continuar para o backend:

- [ ] Projeto Supabase criado (região São Paulo)
- [ ] Schema executado sem erros (10 tabelas)
- [ ] Seed data inserido (1 tenant, 1 user, 1 appointment)
- [ ] RLS testado e funcionando
- [ ] Credenciais salvas em `.env.local`
- [ ] Auth config ajustada (email confirmations disabled)
- [ ] Teste via Postman passou (opcional)

---

## 🎯 Próximo Passo

**Agora vamos para:** Setup Backend .NET

**Arquivo:** `SETUP-BACKEND.md` (próximo documento)

---

## 🆘 Problemas Comuns

### Erro: "permission denied for schema public"
**Solução:** Executar no SQL Editor:
```sql
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON ALL TABLES IN SCHEMA public TO postgres;
```

### Erro: "uuid-ossp extension not found"
**Solução:** Já deve estar instalado no Supabase. Se não:
```sql
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### RLS não está bloqueando
**Solução:** Verificar que policies foram criadas:
```sql
SELECT * FROM pg_policies WHERE tablename = 'appointments';
```

---

**Tempo decorrido:** ~30 min  
**Status:** Database pronto para uso! ✅
