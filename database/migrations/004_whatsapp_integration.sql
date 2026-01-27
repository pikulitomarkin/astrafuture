-- Migration 004: WhatsApp Integration & API Keys
-- Cria tabelas para gerenciar integrações via WhatsApp

-- Tabela de API Keys para autenticação de webhooks
CREATE TABLE IF NOT EXISTS api_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key TEXT UNIQUE NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    is_active BOOLEAN NOT NULL DEFAULT true,
    last_used_at TIMESTAMP,
    expires_at TIMESTAMP NOT NULL,
    rate_limit INTEGER,
    usage_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Índices para api_keys
CREATE INDEX IF NOT EXISTS idx_api_keys_tenant ON api_keys(tenant_id);
CREATE INDEX IF NOT EXISTS idx_api_keys_key ON api_keys(key) WHERE is_active = true;
CREATE INDEX IF NOT EXISTS idx_api_keys_expires ON api_keys(expires_at) WHERE is_active = true;

-- Tabela de Leads do WhatsApp (antes de virar Customer)
CREATE TABLE IF NOT EXISTS whatsapp_leads (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    phone_number TEXT NOT NULL,
    name TEXT,
    email TEXT,
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE SET NULL,
    status INTEGER NOT NULL DEFAULT 0, -- 0=New, 1=InProgress, 2=Converted, 3=Lost
    source INTEGER NOT NULL DEFAULT 0, -- 0=WhatsApp, 1=Manual, 2=Import
    notes TEXT,
    converted_at TIMESTAMP,
    metadata JSONB DEFAULT '{}',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Índices para whatsapp_leads
CREATE INDEX IF NOT EXISTS idx_whatsapp_leads_tenant ON whatsapp_leads(tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_leads_phone ON whatsapp_leads(phone_number, tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_leads_status ON whatsapp_leads(status, tenant_id);
CREATE INDEX IF NOT EXISTS idx_whatsapp_leads_customer ON whatsapp_leads(customer_id);

-- Comentários
COMMENT ON TABLE api_keys IS 'Chaves de API para autenticação de webhooks e integrações externas';
COMMENT ON TABLE whatsapp_leads IS 'Leads capturados via WhatsApp antes de serem convertidos em customers';

COMMENT ON COLUMN api_keys.key IS 'Chave de API no formato astrafuture_live_XXXXXXXX';
COMMENT ON COLUMN api_keys.rate_limit IS 'Limite de requisições por minuto (null = sem limite)';
COMMENT ON COLUMN api_keys.usage_count IS 'Contador total de usos da API Key';

COMMENT ON COLUMN whatsapp_leads.status IS '0=Novo, 1=Em Progresso, 2=Convertido, 3=Perdido';
COMMENT ON COLUMN whatsapp_leads.source IS '0=WhatsApp, 1=Manual, 2=Importação';
COMMENT ON COLUMN whatsapp_leads.metadata IS 'Dados adicionais em JSON (ex: última mensagem, preferências)';
