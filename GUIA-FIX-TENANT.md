# 🔧 Guia Rápido: Resolver Erro "Tenant or user not found"

## O Problema

O erro `Tenant or user not found` ocorre porque:
1. Seu usuário logado tem o `tenant_id = 00000000-0000-0000-0000-000000000001` no JWT
2. Mas esse tenant não existe no banco de dados Supabase
3. O RLS (Row-Level Security) bloqueia a conexão quando o tenant não é encontrado

## Solução Rápida (5 minutos)

### Passo 1: Diagnosticar o problema

1. Acesse o **Supabase Dashboard**: https://supabase.com/dashboard/project/alxtzjmtclopraayehfg
2. Clique em **SQL Editor** no menu lateral
3. Execute o arquivo `check-tenant-issue.sql` (copie e cole o conteúdo)
4. Veja quais tenants e usuários existem

### Passo 2: Escolher a solução

#### Opção A: Criar o tenant padrão (RECOMENDADO)

Execute no **SQL Editor**:

```sql
-- Criar tenant padrão
INSERT INTO tenants (
    id,
    name,
    slug,
    tenant_type,
    is_active,
    created_at,
    updated_at
)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Minha Empresa',
    'minha-empresa',
    'psychology',
    true,
    NOW(),
    NOW()
)
ON CONFLICT (id) DO NOTHING;
```

Depois, vincule seu usuário a este tenant:

```sql
-- Primeiro, pegue seu auth_user_id
SELECT id as auth_user_id, email FROM auth.users WHERE email = 'seu-email@example.com';

-- Depois, crie/atualize o usuário na tabela users
INSERT INTO users (
    id,
    tenant_id,
    auth_user_id,
    email,
    full_name,
    role,
    is_active,
    created_at,
    updated_at
)
VALUES (
    gen_random_uuid(),
    '00000000-0000-0000-0000-000000000001',
    'COLE-SEU-AUTH-USER-ID-AQUI', -- Do SELECT acima
    'seu-email@example.com',
    'Seu Nome',
    'owner',
    true,
    NOW(),
    NOW()
)
ON CONFLICT (auth_user_id) DO UPDATE SET
    tenant_id = '00000000-0000-0000-0000-000000000001',
    role = 'owner';
```

#### Opção B: Usar um tenant existente

Se já existe um tenant no banco:

1. Execute `check-tenant-issue.sql` e veja o ID do tenant existente
2. Atualize o backend para usar esse tenant_id no JWT
3. Ou atualize seu usuário para pertencer ao tenant correto:

```sql
UPDATE users 
SET tenant_id = 'ID-DO-TENANT-EXISTENTE'
WHERE auth_user_id = 'SEU-AUTH-USER-ID';
```

#### Opção C: Desabilitar RLS temporariamente (APENAS DESENVOLVIMENTO!)

⚠️ **NÃO USE EM PRODUÇÃO!**

```sql
-- Desabilitar RLS nas tabelas problemáticas
ALTER TABLE api_keys DISABLE ROW LEVEL SECURITY;
ALTER TABLE tenants DISABLE ROW LEVEL SECURITY;
ALTER TABLE users DISABLE ROW LEVEL SECURITY;
```

Para reabilitar depois:
```sql
ALTER TABLE api_keys ENABLE ROW LEVEL SECURITY;
ALTER TABLE tenants ENABLE ROW LEVEL SECURITY;
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
```

### Passo 3: Verificar se funcionou

Execute no **SQL Editor**:

```sql
SELECT 
    t.id as tenant_id,
    t.name as tenant_name,
    u.email,
    u.role
FROM tenants t
INNER JOIN users u ON t.id = u.tenant_id
WHERE t.id = '00000000-0000-0000-0000-000000000001';
```

Deve retornar pelo menos 1 linha com seu usuário.

### Passo 4: Testar na dashboard

1. Faça **logout** da dashboard
2. Faça **login** novamente (para obter novo JWT com dados corretos)
3. Tente criar a API key novamente
4. ✅ Deve funcionar!

## Por que isso aconteceu?

O sistema usa **multi-tenancy com RLS**, então:
- Todo usuário precisa pertencer a um tenant
- O `tenant_id` vai no JWT
- O Supabase valida se o tenant existe antes de permitir queries
- Se o tenant não existir, a conexão é bloqueada

## Próximos Passos

Depois de criar o tenant e vincular o usuário:

1. ✅ Criar API keys funcionará
2. ✅ Você poderá configurar o WhatsApp bot
3. ✅ Todas as operações multi-tenant funcionarão corretamente

---

**Dúvidas?** Execute os scripts de diagnóstico e me envie o resultado!
