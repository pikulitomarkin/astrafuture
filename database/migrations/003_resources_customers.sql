-- =====================================================
-- Migration: Add Resources table (if not exists)
-- Date: 2026-01-16
-- =====================================================

-- Resources table (profissionais, salas, equipamentos)
CREATE TABLE IF NOT EXISTS resources (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
    
    name VARCHAR(255) NOT NULL,
    description TEXT,
    resource_type INT NOT NULL DEFAULT 1, -- 1=Professional, 2=Room, 3=Equipment
    
    email VARCHAR(255),
    phone VARCHAR(50),
    color VARCHAR(7) DEFAULT '#3B82F6',
    
    meta_data JSONB DEFAULT '{}',
    is_active BOOLEAN DEFAULT true,
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_resources_tenant ON resources(tenant_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_resources_type ON resources(tenant_id, resource_type) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS idx_resources_active ON resources(tenant_id, is_active) WHERE deleted_at IS NULL;

-- =====================================================
-- Customers table updates (ensure compatibility)
-- =====================================================

-- Add missing columns if they don't exist
DO $$ 
BEGIN
    -- Add name column if missing (maps to full_name)
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'customers' AND column_name = 'full_name') THEN
        ALTER TABLE customers ADD COLUMN full_name VARCHAR(255);
    END IF;
END $$;

-- =====================================================
-- Sample data for testing
-- =====================================================

-- Insert default tenant if not exists
INSERT INTO tenants (id, name, slug, tenant_type)
VALUES ('00000000-0000-0000-0000-000000000001', 'AstraFuture Demo', 'astrafuture-demo', 'general')
ON CONFLICT (id) DO NOTHING;

-- Insert sample resources
INSERT INTO resources (id, tenant_id, name, resource_type, description, email, color, is_active)
VALUES 
    ('00000000-0000-0000-0000-000000000101', '00000000-0000-0000-0000-000000000001', 'Dr. João Silva', 1, 'Psicólogo especialista em terapia cognitiva', 'joao@astrafuture.com', '#3B82F6', true),
    ('00000000-0000-0000-0000-000000000102', '00000000-0000-0000-0000-000000000001', 'Dra. Maria Santos', 1, 'Psicóloga especialista em ansiedade', 'maria@astrafuture.com', '#10B981', true),
    ('00000000-0000-0000-0000-000000000103', '00000000-0000-0000-0000-000000000001', 'Sala 1', 2, 'Sala de atendimento principal', NULL, '#F59E0B', true),
    ('00000000-0000-0000-0000-000000000104', '00000000-0000-0000-0000-000000000001', 'Sala 2', 2, 'Sala de atendimento secundária', NULL, '#EF4444', true)
ON CONFLICT (id) DO NOTHING;

-- Insert sample customers
INSERT INTO customers (id, tenant_id, full_name, email, phone, is_active)
VALUES 
    ('00000000-0000-0000-0000-000000000201', '00000000-0000-0000-0000-000000000001', 'Ana Costa', 'ana@email.com', '11999991111', true),
    ('00000000-0000-0000-0000-000000000202', '00000000-0000-0000-0000-000000000001', 'Carlos Oliveira', 'carlos@email.com', '11999992222', true),
    ('00000000-0000-0000-0000-000000000203', '00000000-0000-0000-0000-000000000001', 'Beatriz Lima', 'beatriz@email.com', '11999993333', true)
ON CONFLICT (id) DO NOTHING;

SELECT 'Migration completed successfully!' as status;
