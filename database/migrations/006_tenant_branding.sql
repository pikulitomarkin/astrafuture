-- Migration 006: Tenant Branding - Garantir campos de personalização
-- Adiciona/atualiza campos para personalização de marca no tenant

-- Garantir que logo_url existe (já deveria existir do schema base)
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'tenants' AND column_name = 'logo_url'
    ) THEN
        ALTER TABLE tenants ADD COLUMN logo_url TEXT;
    END IF;
END $$;

-- Garantir que primary_color existe
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'tenants' AND column_name = 'primary_color'
    ) THEN
        ALTER TABLE tenants ADD COLUMN primary_color VARCHAR(7) DEFAULT '#3B82F6';
    END IF;
END $$;

-- Adicionar índice para busca por slug
CREATE INDEX IF NOT EXISTS idx_tenants_slug ON tenants(slug);

-- Comentários
COMMENT ON COLUMN tenants.logo_url IS 'URL do logo customizado da empresa (substitui logo padrão do Astra)';
COMMENT ON COLUMN tenants.primary_color IS 'Cor primária da marca em hexadecimal (#RRGGBB)';
COMMENT ON COLUMN tenants.name IS 'Nome da empresa/organização exibido na interface';
