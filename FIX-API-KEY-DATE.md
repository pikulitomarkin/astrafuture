# Fix: API Key com Data de Expiração Incorreta

## Problema
API Keys estão sendo criadas com data de expiração incorreta (ano 0001) porque as colunas do banco estão como `TIMESTAMP` sem timezone.

## Solução

### 1. Executar Migration no Supabase

Abra o **Supabase SQL Editor** e execute:

```sql
-- Migration 005: Fix TIMESTAMP columns to TIMESTAMPTZ
ALTER TABLE api_keys 
    ALTER COLUMN last_used_at TYPE TIMESTAMPTZ USING last_used_at AT TIME ZONE 'UTC',
    ALTER COLUMN expires_at TYPE TIMESTAMPTZ USING expires_at AT TIME ZONE 'UTC',
    ALTER COLUMN created_at TYPE TIMESTAMPTZ USING created_at AT TIME ZONE 'UTC',
    ALTER COLUMN updated_at TYPE TIMESTAMPTZ USING updated_at AT TIME ZONE 'UTC';

ALTER TABLE whatsapp_leads
    ALTER COLUMN converted_at TYPE TIMESTAMPTZ USING converted_at AT TIME ZONE 'UTC',
    ALTER COLUMN created_at TYPE TIMESTAMPTZ USING created_at AT TIME ZONE 'UTC',
    ALTER COLUMN updated_at TYPE TIMESTAMPTZ USING updated_at AT TIME ZONE 'UTC';
```

### 2. Deletar API Keys Antigas (Opcional)

Se quiser limpar as API keys com data incorreta:

```sql
DELETE FROM api_keys WHERE EXTRACT(YEAR FROM expires_at) < 2000;
```

### 3. Criar Nova API Key

Depois de executar a migration, crie uma nova API key pelo dashboard. Agora a data de expiração será salva corretamente.

## Verificação

Para verificar se está correto:

```sql
SELECT name, expires_at, 
       EXTRACT(YEAR FROM expires_at) as ano,
       expires_at > NOW() as ativa
FROM api_keys;
```

Deve mostrar ano 2027 (ou conforme os dias configurados).
