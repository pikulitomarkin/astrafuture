-- =====================================================
-- AstraFuture - Database Schema (PostgreSQL 15+)
-- Multi-tenant Architecture with Row-Level Security
-- =====================================================

-- Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For fuzzy search

-- =====================================================
-- CORE TABLES
-- =====================================================

-- Tenants: Cada organização é um tenant isolado
CREATE TABLE tenants (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    
    -- Identity
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(100) UNIQUE NOT NULL, -- Ex: "clinica-psique", "construtora-xpto"
    
    -- Configuration
    tenant_type VARCHAR(50) NOT NULL, -- 'psychology', 'law', 'construction', 'aesthetics'
    timezone VARCHAR(50) DEFAULT 'America/Sao_Paulo',
    locale VARCHAR(10) DEFAULT 'pt-BR',
    
    -- Business Rules (JSON Schema)
    meta_schema JSONB DEFAULT '{}', -- Define campos customizados por tipo
    business_rules JSONB DEFAULT '{}', -- Regras específicas do negócio
    
    -- Subscription & Limits
    subscription_tier VARCHAR(50) DEFAULT 'free', -- 'free', 'pro', 'enterprise'
    max_users INT DEFAULT 3,
    max_appointments_per_month INT DEFAULT 100,
    
    -- Branding
    logo_url TEXT,
    primary_color VARCHAR(7) DEFAULT '#3B82F6',
    custom_domain VARCHAR(255), -- Ex: agendamentos.clinicapsique.com.br
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    onboarding_completed_at TIMESTAMPTZ,
    
    -- Audit
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ -- Soft delete
);

-- Indexes
CREATE INDEX idx_tenants_slug ON tenants(slug) WHERE deleted_at IS NULL;
CREATE INDEX idx_tenants_type ON tenants(tenant_type) WHERE is_active = true;

-- =====================================================

-- Users: Usuários vinculados a um tenant
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Identity (sincronizado com Supabase Auth)
    auth_user_id UUID NOT NULL UNIQUE, -- Referência ao Supabase auth.users.id
    email VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    
    -- Profile
    full_name VARCHAR(255) NOT NULL,
    avatar_url TEXT,
    bio TEXT,
    
    -- Role & Permissions
    role VARCHAR(50) NOT NULL DEFAULT 'member', -- 'owner', 'admin', 'member', 'guest'
    permissions JSONB DEFAULT '[]', -- ['appointments:write', 'reports:read']
    
    -- Professional Info (para prestadores de serviço)
    professional_title VARCHAR(100), -- 'Psicóloga', 'Advogado', 'Arquiteto'
    professional_license VARCHAR(100), -- CRP, OAB, CAU
    specialties TEXT[], -- ['Terapia Cognitiva', 'Ansiedade']
    
    -- Preferences
    notification_preferences JSONB DEFAULT '{"email": true, "sms": false, "push": true}',
    language VARCHAR(10) DEFAULT 'pt-BR',
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    email_verified_at TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    
    -- Audit
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    -- Constraints
    CONSTRAINT unique_email_per_tenant UNIQUE(tenant_id, email)
);

-- Indexes
CREATE INDEX idx_users_tenant ON users(tenant_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_users_email ON users(email) WHERE is_active = true;
CREATE INDEX idx_users_auth ON users(auth_user_id);
CREATE INDEX idx_users_role ON users(tenant_id, role);

-- =====================================================

-- Resources: Recursos agendáveis (profissionais, salas, equipamentos)
CREATE TABLE resources (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Identity
    name VARCHAR(255) NOT NULL,
    resource_type VARCHAR(50) NOT NULL, -- 'professional', 'room', 'equipment'
    
    -- Association
    user_id UUID REFERENCES users(id) ON DELETE SET NULL, -- Se for um profissional
    
    -- Availability
    working_hours JSONB DEFAULT '{}', -- { "monday": [{"start": "09:00", "end": "18:00"}] }
    booking_buffer_minutes INT DEFAULT 0, -- Tempo entre appointments
    
    -- Capacity
    max_simultaneous_bookings INT DEFAULT 1,
    
    -- Metadata
    description TEXT,
    location VARCHAR(255),
    meta_data JSONB DEFAULT '{}',
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    
    -- Audit
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

-- Indexes
CREATE INDEX idx_resources_tenant ON resources(tenant_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_resources_type ON resources(tenant_id, resource_type);
CREATE INDEX idx_resources_user ON resources(user_id);

-- =====================================================

-- Customers: Clientes que agendam serviços
CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Identity
    full_name VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(20) NOT NULL, -- Principal meio de contato
    
    -- Profile
    birth_date DATE,
    document_number VARCHAR(50), -- CPF, CNPJ
    
    -- Address
    address JSONB DEFAULT '{}', -- { "street", "number", "city", "state", "zip" }
    
    -- Source
    lead_source VARCHAR(50), -- 'whatsapp', 'website', 'referral', 'instagram'
    referred_by UUID REFERENCES customers(id),
    
    -- Metadata (campos customizados por tenant)
    meta_data JSONB DEFAULT '{}',
    
    -- Marketing
    accepts_marketing BOOLEAN DEFAULT false,
    tags TEXT[], -- ['vip', 'inadimplente', 'primeira-consulta']
    
    -- Status
    is_active BOOLEAN DEFAULT true,
    
    -- Audit
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ,
    
    -- Constraints
    CONSTRAINT unique_phone_per_tenant UNIQUE(tenant_id, phone)
);

-- Indexes
CREATE INDEX idx_customers_tenant ON customers(tenant_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_customers_phone ON customers(phone);
CREATE INDEX idx_customers_email ON customers(email);
CREATE INDEX idx_customers_tags ON customers USING GIN(tags);
CREATE INDEX idx_customers_name_trgm ON customers USING GIN(full_name gin_trgm_ops);

-- =====================================================

-- Appointments: Agendamentos (núcleo do sistema)
CREATE TABLE appointments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Relationships
    customer_id UUID NOT NULL REFERENCES customers(id) ON DELETE RESTRICT,
    resource_id UUID NOT NULL REFERENCES resources(id) ON DELETE RESTRICT,
    assigned_to UUID REFERENCES users(id) ON DELETE SET NULL, -- Profissional responsável
    
    -- Schedule
    scheduled_at TIMESTAMPTZ NOT NULL,
    duration_minutes INT NOT NULL DEFAULT 60,
    ends_at TIMESTAMPTZ GENERATED ALWAYS AS (scheduled_at + (duration_minutes || ' minutes')::INTERVAL) STORED,
    
    -- Details
    title VARCHAR(255) NOT NULL,
    description TEXT,
    appointment_type VARCHAR(50), -- 'initial', 'follow-up', 'emergency', 'online'
    location VARCHAR(255), -- 'Sala 101', 'Online (Google Meet)', 'Obra X'
    
    -- Status Flow
    status VARCHAR(50) DEFAULT 'scheduled', -- 'scheduled', 'confirmed', 'in_progress', 'completed', 'cancelled', 'no_show'
    status_updated_at TIMESTAMPTZ DEFAULT NOW(),
    cancellation_reason TEXT,
    cancelled_by UUID REFERENCES users(id),
    cancelled_at TIMESTAMPTZ,
    
    -- Meeting
    meeting_url TEXT, -- Link do Google Meet, Zoom, etc
    meeting_notes TEXT, -- Notas da consulta/reunião
    
    -- Payment
    price_cents INT, -- Valor em centavos (ex: 15000 = R$ 150,00)
    payment_status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'paid', 'refunded'
    paid_at TIMESTAMPTZ,
    
    -- Notifications
    reminder_sent_at TIMESTAMPTZ,
    confirmation_sent_at TIMESTAMPTZ,
    
    -- Metadata (campos específicos do tenant)
    meta_data JSONB DEFAULT '{}',
    
    -- Audit
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ
);

-- Indexes (CRÍTICO para performance)
CREATE INDEX idx_appointments_tenant_date ON appointments(tenant_id, scheduled_at DESC) WHERE deleted_at IS NULL;
CREATE INDEX idx_appointments_customer ON appointments(customer_id);
CREATE INDEX idx_appointments_resource_date ON appointments(resource_id, scheduled_at);
CREATE INDEX idx_appointments_status ON appointments(tenant_id, status) WHERE deleted_at IS NULL;
CREATE INDEX idx_appointments_assigned ON appointments(assigned_to);

-- Full-text search
CREATE INDEX idx_appointments_search ON appointments USING GIN(
    to_tsvector('portuguese', COALESCE(title, '') || ' ' || COALESCE(description, ''))
);

-- =====================================================

-- Appointment History: Auditoria de mudanças
CREATE TABLE appointment_history (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    appointment_id UUID NOT NULL REFERENCES appointments(id) ON DELETE CASCADE,
    
    -- Change tracking
    changed_by UUID REFERENCES users(id),
    change_type VARCHAR(50) NOT NULL, -- 'created', 'rescheduled', 'cancelled', 'status_changed'
    old_values JSONB,
    new_values JSONB,
    
    -- Context
    reason TEXT,
    ip_address INET,
    user_agent TEXT,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_appointment_history_appointment ON appointment_history(appointment_id);
CREATE INDEX idx_appointment_history_date ON appointment_history(created_at DESC);

-- =====================================================

-- Availability Exceptions: Exceções de disponibilidade
CREATE TABLE availability_exceptions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    resource_id UUID NOT NULL REFERENCES resources(id) ON DELETE CASCADE,
    
    -- Period
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    all_day BOOLEAN DEFAULT true,
    start_time TIME,
    end_time TIME,
    
    -- Type
    exception_type VARCHAR(50) NOT NULL, -- 'unavailable', 'holiday', 'special_hours'
    reason VARCHAR(255),
    
    -- Recurrence
    is_recurring BOOLEAN DEFAULT false,
    recurrence_rule TEXT, -- RRULE format (RFC 5545)
    
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_availability_exceptions_resource ON availability_exceptions(resource_id);
CREATE INDEX idx_availability_exceptions_dates ON availability_exceptions(start_date, end_date);

-- =====================================================

-- Notifications: Fila de notificações
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Target
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    customer_id UUID REFERENCES customers(id) ON DELETE CASCADE,
    
    -- Content
    type VARCHAR(50) NOT NULL, -- 'appointment_reminder', 'appointment_confirmed', 'payment_received'
    channel VARCHAR(50) NOT NULL, -- 'email', 'sms', 'whatsapp', 'push', 'in_app'
    subject VARCHAR(255),
    body TEXT NOT NULL,
    
    -- Metadata
    related_appointment_id UUID REFERENCES appointments(id),
    template_id VARCHAR(100),
    
    -- Delivery
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'sent', 'delivered', 'failed'
    scheduled_for TIMESTAMPTZ DEFAULT NOW(),
    sent_at TIMESTAMPTZ,
    delivered_at TIMESTAMPTZ,
    error_message TEXT,
    retry_count INT DEFAULT 0,
    
    -- Tracking
    opened_at TIMESTAMPTZ,
    clicked_at TIMESTAMPTZ,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_notifications_tenant ON notifications(tenant_id);
CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_status ON notifications(status, scheduled_for);
CREATE INDEX idx_notifications_appointment ON notifications(related_appointment_id);

-- =====================================================

-- Webhook Events: Para integrações externas
CREATE TABLE webhook_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Event
    event_type VARCHAR(100) NOT NULL, -- 'appointment.created', 'appointment.cancelled'
    payload JSONB NOT NULL,
    
    -- Target
    webhook_url TEXT NOT NULL,
    
    -- Delivery
    status VARCHAR(50) DEFAULT 'pending', -- 'pending', 'delivered', 'failed'
    attempts INT DEFAULT 0,
    last_attempt_at TIMESTAMPTZ,
    next_retry_at TIMESTAMPTZ,
    response_status_code INT,
    response_body TEXT,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_webhook_events_tenant ON webhook_events(tenant_id);
CREATE INDEX idx_webhook_events_status ON webhook_events(status, next_retry_at);

-- =====================================================

-- Audit Log: Log completo de ações no sistema
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES tenants(id) ON DELETE CASCADE,
    
    -- Actor
    user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    
    -- Action
    action VARCHAR(100) NOT NULL, -- 'user.login', 'appointment.create', 'settings.update'
    entity_type VARCHAR(50), -- 'appointment', 'customer', 'user'
    entity_id UUID,
    
    -- Context
    changes JSONB, -- Before/after values
    metadata JSONB DEFAULT '{}',
    
    -- Request Info
    ip_address INET,
    user_agent TEXT,
    request_id UUID,
    
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_tenant ON audit_logs(tenant_id, created_at DESC);
CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);

-- =====================================================
-- ROW-LEVEL SECURITY (RLS) POLICIES
-- =====================================================

-- Habilitar RLS em todas as tabelas multi-tenant
ALTER TABLE tenants ENABLE ROW LEVEL SECURITY;
ALTER TABLE users ENABLE ROW LEVEL SECURITY;
ALTER TABLE resources ENABLE ROW LEVEL SECURITY;
ALTER TABLE customers ENABLE ROW LEVEL SECURITY;
ALTER TABLE appointments ENABLE ROW LEVEL SECURITY;
ALTER TABLE appointment_history ENABLE ROW LEVEL SECURITY;
ALTER TABLE availability_exceptions ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications ENABLE ROW LEVEL SECURITY;
ALTER TABLE webhook_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit_logs ENABLE ROW LEVEL SECURITY;

-- Helper function para obter tenant_id do contexto
CREATE OR REPLACE FUNCTION get_current_tenant_id()
RETURNS UUID AS $$
BEGIN
    RETURN NULLIF(current_setting('app.tenant_id', true), '')::UUID;
END;
$$ LANGUAGE plpgsql STABLE;

-- =====================================================
-- POLICIES: Tenants
-- =====================================================

CREATE POLICY "Users can view their own tenant"
ON tenants FOR SELECT
USING (id = get_current_tenant_id());

CREATE POLICY "Admins can update their tenant"
ON tenants FOR UPDATE
USING (
    id = get_current_tenant_id()
    AND EXISTS (
        SELECT 1 FROM users
        WHERE users.tenant_id = tenants.id
        AND users.auth_user_id = auth.uid()
        AND users.role IN ('owner', 'admin')
    )
);

-- =====================================================
-- POLICIES: Users
-- =====================================================

CREATE POLICY "Users can view users from their tenant"
ON users FOR SELECT
USING (tenant_id = get_current_tenant_id());

CREATE POLICY "Users can update their own profile"
ON users FOR UPDATE
USING (auth_user_id = auth.uid());

CREATE POLICY "Admins can manage users in their tenant"
ON users FOR ALL
USING (
    tenant_id = get_current_tenant_id()
    AND EXISTS (
        SELECT 1 FROM users u
        WHERE u.tenant_id = users.tenant_id
        AND u.auth_user_id = auth.uid()
        AND u.role IN ('owner', 'admin')
    )
);

-- =====================================================
-- POLICIES: Resources
-- =====================================================

CREATE POLICY "Users can view resources from their tenant"
ON resources FOR SELECT
USING (tenant_id = get_current_tenant_id());

CREATE POLICY "Admins can manage resources"
ON resources FOR ALL
USING (
    tenant_id = get_current_tenant_id()
    AND EXISTS (
        SELECT 1 FROM users
        WHERE users.tenant_id = resources.tenant_id
        AND users.auth_user_id = auth.uid()
        AND users.role IN ('owner', 'admin')
    )
);

-- =====================================================
-- POLICIES: Customers
-- =====================================================

CREATE POLICY "Users can view customers from their tenant"
ON customers FOR SELECT
USING (tenant_id = get_current_tenant_id());

CREATE POLICY "Users can create customers in their tenant"
ON customers FOR INSERT
WITH CHECK (tenant_id = get_current_tenant_id());

CREATE POLICY "Users can update customers in their tenant"
ON customers FOR UPDATE
USING (tenant_id = get_current_tenant_id());

-- =====================================================
-- POLICIES: Appointments
-- =====================================================

CREATE POLICY "Users can view appointments from their tenant"
ON appointments FOR SELECT
USING (tenant_id = get_current_tenant_id());

CREATE POLICY "Users can create appointments in their tenant"
ON appointments FOR INSERT
WITH CHECK (
    tenant_id = get_current_tenant_id()
    AND created_by IN (
        SELECT id FROM users WHERE auth_user_id = auth.uid()
    )
);

CREATE POLICY "Users can update appointments in their tenant"
ON appointments FOR UPDATE
USING (tenant_id = get_current_tenant_id());

CREATE POLICY "Users can delete their own appointments"
ON appointments FOR DELETE
USING (
    tenant_id = get_current_tenant_id()
    AND created_by IN (
        SELECT id FROM users WHERE auth_user_id = auth.uid()
    )
);

-- =====================================================
-- Políticas semelhantes para demais tabelas...
-- (omitidas para brevidade, mas seguem mesmo padrão)
-- =====================================================

-- =====================================================
-- FUNCTIONS & TRIGGERS
-- =====================================================

-- Trigger para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Aplicar trigger em todas as tabelas relevantes
CREATE TRIGGER update_tenants_updated_at BEFORE UPDATE ON tenants
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_resources_updated_at BEFORE UPDATE ON resources
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_customers_updated_at BEFORE UPDATE ON customers
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_appointments_updated_at BEFORE UPDATE ON appointments
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_availability_exceptions_updated_at BEFORE UPDATE ON availability_exceptions
    FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

-- =====================================================

-- Trigger para criar audit log automaticamente
CREATE OR REPLACE FUNCTION create_audit_log()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_logs (
        tenant_id,
        user_id,
        action,
        entity_type,
        entity_id,
        changes,
        request_id
    ) VALUES (
        COALESCE(NEW.tenant_id, OLD.tenant_id),
        (SELECT id FROM users WHERE auth_user_id = auth.uid() LIMIT 1),
        TG_TABLE_NAME || '.' || lower(TG_OP),
        TG_TABLE_NAME,
        COALESCE(NEW.id, OLD.id),
        jsonb_build_object('old', to_jsonb(OLD), 'new', to_jsonb(NEW)),
        NULLIF(current_setting('app.request_id', true), '')::UUID
    );
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Aplicar audit trigger em appointments (exemplo)
CREATE TRIGGER audit_appointments
AFTER INSERT OR UPDATE OR DELETE ON appointments
FOR EACH ROW EXECUTE FUNCTION create_audit_log();

-- =====================================================

-- Function para verificar conflitos de agendamento
CREATE OR REPLACE FUNCTION check_appointment_conflict(
    p_resource_id UUID,
    p_scheduled_at TIMESTAMPTZ,
    p_duration_minutes INT,
    p_exclude_appointment_id UUID DEFAULT NULL
)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (
        SELECT 1 FROM appointments
        WHERE resource_id = p_resource_id
        AND id != COALESCE(p_exclude_appointment_id, '00000000-0000-0000-0000-000000000000'::UUID)
        AND status NOT IN ('cancelled', 'no_show')
        AND deleted_at IS NULL
        AND (
            -- Novo appointment começa durante existente
            (p_scheduled_at >= scheduled_at AND p_scheduled_at < ends_at)
            OR
            -- Novo appointment termina durante existente
            (p_scheduled_at + (p_duration_minutes || ' minutes')::INTERVAL > scheduled_at 
             AND p_scheduled_at + (p_duration_minutes || ' minutes')::INTERVAL <= ends_at)
            OR
            -- Novo appointment engloba existente
            (p_scheduled_at <= scheduled_at 
             AND p_scheduled_at + (p_duration_minutes || ' minutes')::INTERVAL >= ends_at)
        )
    );
END;
$$ LANGUAGE plpgsql STABLE;

-- =====================================================

-- Function para calcular próximos horários disponíveis
CREATE OR REPLACE FUNCTION get_available_slots(
    p_tenant_id UUID,
    p_resource_id UUID,
    p_date DATE,
    p_duration_minutes INT DEFAULT 60
)
RETURNS TABLE (
    slot_time TIMESTAMPTZ,
    is_available BOOLEAN
) AS $$
BEGIN
    -- Implementação complexa: considera working_hours, exceptions, buffer, etc
    -- (Omitida para brevidade - seria função complexa de 100+ linhas)
    RETURN QUERY SELECT NOW()::TIMESTAMPTZ, true LIMIT 0;
END;
$$ LANGUAGE plpgsql STABLE;

-- =====================================================
-- VIEWS (para simplificar queries comuns)
-- =====================================================

-- View de appointments com informações relacionadas
CREATE VIEW appointments_detailed AS
SELECT 
    a.*,
    c.full_name AS customer_name,
    c.email AS customer_email,
    c.phone AS customer_phone,
    r.name AS resource_name,
    r.resource_type,
    u.full_name AS assigned_to_name,
    t.name AS tenant_name,
    t.timezone AS tenant_timezone
FROM appointments a
JOIN customers c ON a.customer_id = c.id
JOIN resources r ON a.resource_id = r.id
LEFT JOIN users u ON a.assigned_to = u.id
JOIN tenants t ON a.tenant_id = t.id
WHERE a.deleted_at IS NULL;

-- View de estatísticas do tenant
CREATE VIEW tenant_statistics AS
SELECT 
    t.id AS tenant_id,
    t.name AS tenant_name,
    COUNT(DISTINCT u.id) AS total_users,
    COUNT(DISTINCT c.id) AS total_customers,
    COUNT(DISTINCT a.id) FILTER (WHERE a.created_at >= NOW() - INTERVAL '30 days') AS appointments_last_30_days,
    COUNT(DISTINCT a.id) FILTER (WHERE a.status = 'completed' AND a.created_at >= NOW() - INTERVAL '30 days') AS completed_appointments_last_30_days,
    SUM(a.price_cents) FILTER (WHERE a.payment_status = 'paid' AND a.created_at >= NOW() - INTERVAL '30 days') AS revenue_last_30_days_cents
FROM tenants t
LEFT JOIN users u ON t.id = u.tenant_id AND u.deleted_at IS NULL
LEFT JOIN customers c ON t.id = c.tenant_id AND c.deleted_at IS NULL
LEFT JOIN appointments a ON t.id = a.tenant_id AND a.deleted_at IS NULL
WHERE t.deleted_at IS NULL
GROUP BY t.id, t.name;

-- =====================================================
-- SEED DATA (exemplo para desenvolvimento)
-- =====================================================

-- Inserir tenant de exemplo
INSERT INTO tenants (id, name, slug, tenant_type, business_rules) VALUES
('11111111-1111-1111-1111-111111111111', 'Clínica Psique', 'clinica-psique', 'psychology', '{
    "min_duration_minutes": 50,
    "allow_online_sessions": true,
    "require_consent_form": true
}'::jsonb);

-- =====================================================
-- COMENTÁRIOS FINAIS
-- =====================================================

-- Performance Tips:
-- 1. Sempre filtrar por tenant_id primeiro (usa index)
-- 2. Usar EXPLAIN ANALYZE para queries complexas
-- 3. Monitorar pg_stat_statements para identificar slow queries
-- 4. Considerar particionamento de appointments quando > 10M registros

-- Security Tips:
-- 1. SEMPRE definir app.tenant_id no contexto da requisição
-- 2. Jamais desabilitar RLS em produção
-- 3. Auditar mudanças em tenant_id (detectar tentativas de bypass)
-- 4. Usar prepared statements para prevenir SQL injection

-- Scalability Tips:
-- 1. Arquivar appointments antigos (> 2 anos) para tabela separada
-- 2. Usar read replicas para relatórios pesados
-- 3. Implementar cache de working_hours e business_rules
-- 4. Considerar TimescaleDB para tabelas de séries temporais (audit_logs, notifications)
