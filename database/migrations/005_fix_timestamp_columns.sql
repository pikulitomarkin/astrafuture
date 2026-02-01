-- Migration 005: Fix TIMESTAMP columns to TIMESTAMPTZ
-- Converte colunas TIMESTAMP para TIMESTAMPTZ para preservar timezone

-- API Keys - alterar colunas de data
ALTER TABLE api_keys 
    ALTER COLUMN last_used_at TYPE TIMESTAMPTZ USING last_used_at AT TIME ZONE 'UTC',
    ALTER COLUMN expires_at TYPE TIMESTAMPTZ USING expires_at AT TIME ZONE 'UTC',
    ALTER COLUMN created_at TYPE TIMESTAMPTZ USING created_at AT TIME ZONE 'UTC',
    ALTER COLUMN updated_at TYPE TIMESTAMPTZ USING updated_at AT TIME ZONE 'UTC';

-- WhatsApp Leads - alterar colunas de data
ALTER TABLE whatsapp_leads
    ALTER COLUMN converted_at TYPE TIMESTAMPTZ USING converted_at AT TIME ZONE 'UTC',
    ALTER COLUMN created_at TYPE TIMESTAMPTZ USING created_at AT TIME ZONE 'UTC',
    ALTER COLUMN updated_at TYPE TIMESTAMPTZ USING updated_at AT TIME ZONE 'UTC';

-- Comentário
COMMENT ON COLUMN api_keys.expires_at IS 'Data de expiração da API key (UTC)';
COMMENT ON COLUMN api_keys.last_used_at IS 'Último uso da API key (UTC)';
