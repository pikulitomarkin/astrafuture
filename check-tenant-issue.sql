-- Script de diagnóstico para problemas de tenant
-- Execute no Supabase SQL Editor

-- 1. Verificar tenants existentes
SELECT 
    id,
    name,
    slug,
    tenant_type,
    is_active,
    created_at
FROM tenants
WHERE deleted_at IS NULL
ORDER BY created_at DESC;

-- 2. Verificar users e seus tenants
SELECT 
    u.id,
    u.tenant_id,
    u.auth_user_id,
    u.email,
    u.full_name,
    u.role,
    t.name as tenant_name
FROM users u
LEFT JOIN tenants t ON u.tenant_id = t.id
WHERE u.deleted_at IS NULL
ORDER BY u.created_at DESC;

-- 3. Verificar se o tenant padrão existe
SELECT 
    EXISTS(
        SELECT 1 FROM tenants 
        WHERE id = '00000000-0000-0000-0000-000000000001'
    ) as tenant_exists;

-- 4. Verificar auth.users do Supabase
SELECT 
    id,
    email,
    created_at,
    last_sign_in_at
FROM auth.users
ORDER BY created_at DESC
LIMIT 10;

-- 5. Verificar se há algum usuário atual logado
SELECT 
    auth.uid() as current_auth_user_id,
    current_user as db_user;
