-- Script para corrigir problema de tenant e usuários
-- Execute no Supabase SQL Editor após rodar check-tenant-issue.sql

-- IMPORTANTE: Substitua os valores conforme necessário!

-- ============================================
-- OPÇÃO 1: Criar tenant padrão se não existir
-- ============================================

-- Criar o tenant padrão (se não existir)
INSERT INTO tenants (
    id,
    name,
    slug,
    tenant_type,
    timezone,
    locale,
    is_active,
    subscription_tier,
    max_users,
    max_appointments_per_month,
    created_at,
    updated_at
)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'Default Tenant',
    'default-tenant',
    'psychology',
    'America/Sao_Paulo',
    'pt-BR',
    true,
    'pro',
    10,
    1000,
    NOW(),
    NOW()
)
ON CONFLICT (id) DO UPDATE SET
    updated_at = NOW();

-- ============================================
-- OPÇÃO 2: Vincular usuário existente ao tenant
-- ============================================

-- Primeiro, veja seus usuários auth:
-- SELECT id, email FROM auth.users;

-- Depois, vincule o usuário ao tenant (SUBSTITUA O EMAIL E AUTH_USER_ID!)
-- Exemplo:
/*
INSERT INTO users (
    id,
    tenant_id,
    auth_user_id,
    email,
    full_name,
    role,
    is_active,
    email_verified_at,
    created_at,
    updated_at
)
VALUES (
    gen_random_uuid(),
    '00000000-0000-0000-0000-000000000001',
    'SEU-AUTH-USER-ID-AQUI', -- Pegue da tabela auth.users
    'seu-email@example.com',
    'Seu Nome',
    'owner',
    true,
    NOW(),
    NOW(),
    NOW()
)
ON CONFLICT (auth_user_id) DO UPDATE SET
    tenant_id = '00000000-0000-0000-0000-000000000001',
    role = 'owner',
    updated_at = NOW();
*/

-- ============================================
-- OPÇÃO 3: Desabilitar RLS temporariamente (NÃO RECOMENDADO EM PRODUÇÃO!)
-- ============================================

-- Use apenas para testes em desenvolvimento:
-- ALTER TABLE api_keys DISABLE ROW LEVEL SECURITY;

-- ============================================
-- VERIFICAÇÃO FINAL
-- ============================================

-- Verificar se tudo está correto:
SELECT 
    t.id as tenant_id,
    t.name as tenant_name,
    u.id as user_id,
    u.auth_user_id,
    u.email,
    u.role
FROM tenants t
LEFT JOIN users u ON t.id = u.tenant_id
WHERE t.id = '00000000-0000-0000-0000-000000000001';

-- Verificar políticas RLS da tabela api_keys:
SELECT 
    schemaname,
    tablename,
    policyname,
    permissive,
    roles,
    cmd,
    qual,
    with_check
FROM pg_policies
WHERE tablename = 'api_keys';
