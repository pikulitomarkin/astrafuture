# 🚀 Setup Rápido - 5 Minutos

## Passo 1: Criar Projeto (2 min)

1. Acesse: https://supabase.com
2. Click: **"New Project"**
3. Preencha:
   - **Name:** `astrafuture-prod`
   - **Password:** [GERE UMA SENHA FORTE - ANOTE!]
   - **Region:** `South America (São Paulo)`
4. Click: **"Create new project"**
5. Aguarde ~2 min (provisioning)

---

## Passo 2: Salvar Credenciais (1 min)

1. Vá em: **Settings > API**
2. Copie:
   - **Project URL**
   - **anon public** (API key)
   - **service_role** (API key - secret!)

3. **Abra:** `d:\Astrafuture\.env.local`
4. **Cole os valores:**

```env
SUPABASE_URL=https://[SEU-PROJECT-REF].supabase.co
SUPABASE_ANON_KEY=eyJhbGciOiJIU...
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIU...
DATABASE_URL=postgresql://postgres.[SEU-PROJECT-REF]:[SUA-SENHA]@aws-0-sa-east-1.pooler.supabase.com:6543/postgres
```

5. **Salve** (Ctrl+S)

---

## Passo 3: Executar Schema (2 min)

### No Supabase Dashboard:

1. Click: **SQL Editor** (menu lateral)
2. Click: **"New Query"**
3. **Abra:** `d:\Astrafuture\database\schema.sql` no VS Code
4. **Copie TODO** (Ctrl+A, Ctrl+C)
5. **Cole** no SQL Editor do Supabase
6. Click: **"Run"** (ou Ctrl+Enter)
7. Aguarde ~10 segundos
8. Veja: ✅ **"Success"**

---

## Passo 4: Verificar Tabelas (30 seg)

1. Click: **Table Editor** (menu lateral)
2. Deve ver **10 tabelas:**
   - ✅ tenants
   - ✅ users
   - ✅ customers
   - ✅ resources
   - ✅ appointments
   - ✅ appointment_history
   - ✅ availability_exceptions
   - ✅ notifications
   - ✅ webhook_events
   - ✅ audit_logs

---

## Passo 5: Seed Data (30 seg)

### No SQL Editor:

1. **Nova Query**
2. **Cole este código:**

```sql
-- Tenant demo
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

-- User owner
INSERT INTO users (id, tenant_id, auth_user_id, email, full_name, role, is_active, email_verified_at)
VALUES (
  '00000000-0000-0000-0000-000000000002',
  '00000000-0000-0000-0000-000000000001',
  '00000000-0000-0000-0000-000000000099',
  'demo@clinicapsique.com',
  'Dr. Ana Silva',
  'owner',
  true,
  NOW()
);

-- Resource
INSERT INTO resources (id, tenant_id, name, resource_type, is_active)
VALUES (
  '00000000-0000-0000-0000-000000000003',
  '00000000-0000-0000-0000-000000000001',
  'Dra. Ana Silva - Consultório 1',
  'professional',
  true
);

-- Customer
INSERT INTO customers (id, tenant_id, full_name, email, phone, is_active)
VALUES (
  '00000000-0000-0000-0000-000000000004',
  '00000000-0000-0000-0000-000000000001',
  'João Santos',
  'joao.santos@example.com',
  '+5511999999999',
  true
);

-- Appointment
INSERT INTO appointments (
  id,
  tenant_id,
  customer_id,
  resource_id,
  title,
  scheduled_at,
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
  NOW() + INTERVAL '1 day' + TIME '14:00:00',
  60,
  'scheduled',
  'consultation'
);
```

3. **Run** (Ctrl+Enter)
4. Veja: ✅ **"Success"**

---

## ✅ Pronto! Database 100% configurado

**Teste rápido:**
```sql
SELECT * FROM appointments;
```

Deve retornar: **1 appointment** (João Santos amanhã 14h)

---

## 🎯 Próximo Passo

Agora volte para o VS Code e vamos conectar o backend! 🚀

```bash
cd d:\Astrafuture\backend-src
dotnet build
```
