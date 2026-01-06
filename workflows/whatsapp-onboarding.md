# WhatsApp Onboarding Flow - AstraFuture

## Objetivo

Transformar o primeiro "Oi" no WhatsApp em um tenant configurado e dashboard acessível em **menos de 3 minutos**, sem formulários longos.

---

## Princípios do Flow

1. **Zero Burocracia**: Coletar apenas dados essenciais
2. **Conversacional**: Simular diálogo natural, não formulário
3. **Progressivo**: Usuário pode completar setup depois
4. **Inteligente**: Inferir informações sempre que possível

---

## Arquitetura do Flow

```
WhatsApp (Cliente)
       ↓
Evolution API / WhatsApp Cloud API
       ↓
Webhook → n8n Workflow
       ↓
Backend API (Pilar 1)
       ↓
Database (Supabase)
       ↓
Magic Link via WhatsApp
       ↓
Dashboard (Next.js)
```

---

## Flow Completo (n8n)

### Passo 1: Receber Mensagem Inicial

**Trigger:** Webhook do Evolution API  
**Condição:** Primeiro contato (customer não existe)

**Payload Recebido:**
```json
{
  "event": "messages.upsert",
  "instance": "astrafuture-prod",
  "data": {
    "key": {
      "remoteJid": "5511987654321@s.whatsapp.net",
      "fromMe": false
    },
    "message": {
      "conversation": "Oi"
    },
    "pushName": "Dr. João Silva",
    "messageTimestamp": 1640995200
  }
}
```

---

### Passo 2: Verificar se é Novo Lead

**Node:** HTTP Request → Backend API  
**Endpoint:** `GET /customers/check-phone?phone=5511987654321`

**Response:**
```json
{
  "exists": false
}
```

**Decisão:**
- Se `exists: true` → Redirecionar para flow de "Usuário Existente"
- Se `exists: false` → Continuar onboarding

---

### Passo 3: Mensagem de Boas-Vindas

**Node:** Send WhatsApp Message

**Mensagem:**
```
👋 Olá, Dr. João Silva!

Bem-vindo ao *AstraFuture*, sua plataforma de agendamentos premium.

Vou te ajudar a criar sua conta em menos de 3 minutos! 🚀

Primeiro, me diz: qual o nome da sua clínica ou empresa?

(Ex: "Clínica Psique", "Escritório Silva Advocacia")
```

---

### Passo 4: Coletar Nome do Negócio

**Node:** Wait for Reply (Webhook)

**Response Example:**
```
"Clínica Psique"
```

**Armazenamento Temporário (n8n Memory):**
```json
{
  "phone": "5511987654321",
  "full_name": "Dr. João Silva",
  "tenant_name": "Clínica Psique"
}
```

---

### Passo 5: Inferir Tipo de Negócio (IA)

**Node:** OpenAI API (GPT-4o-mini)

**Prompt:**
```
Classifique o tipo de negócio baseado no nome:
"Clínica Psique"

Retorne APENAS uma dessas opções:
- psychology
- law
- construction
- aesthetics
- healthcare
- education
- other

Formato: JSON {"type": "psychology", "confidence": 0.95}
```

**Response:**
```json
{
  "type": "psychology",
  "confidence": 0.95
}
```

**Decisão:**
- Se `confidence >= 0.8` → Usar tipo inferido
- Se `confidence < 0.8` → Perguntar ao usuário

---

### Passo 6A: Confirmar Tipo (Alta Confiança)

**Mensagem:**
```
Perfeito! Identifiquei que você trabalha com *Psicologia*. ✅

Vou configurar tudo pensando no seu tipo de atendimento!

Agora, qual seu melhor email para login?
(Pode enviar aqui mesmo)
```

---

### Passo 6B: Perguntar Tipo (Baixa Confiança)

**Mensagem:**
```
Legal! E qual área você atua?

1️⃣ Psicologia / Terapia
2️⃣ Advocacia
3️⃣ Construção / Arquitetura
4️⃣ Estética / Beleza
5️⃣ Saúde (médico, dentista)
6️⃣ Educação / Consultoria
7️⃣ Outro

Envie o número da opção!
```

**Mapeamento:**
```javascript
const typeMap = {
  "1": "psychology",
  "2": "law",
  "3": "construction",
  "4": "aesthetics",
  "5": "healthcare",
  "6": "education",
  "7": "other"
};
```

---

### Passo 7: Coletar Email

**Node:** Wait for Reply

**Response Example:**
```
"joao@clinicapsique.com.br"
```

**Validação (n8n):**
```javascript
const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
if (!emailRegex.test(email)) {
  return "❌ Email inválido. Tente novamente:";
}
```

---

### Passo 8: Criar Conta (Backend API)

**Node:** HTTP Request → POST `/auth/register`

**Payload:**
```json
{
  "phone": "+5511987654321",
  "full_name": "Dr. João Silva",
  "email": "joao@clinicapsique.com.br",
  "tenant_name": "Clínica Psique",
  "tenant_type": "psychology",
  "lead_source": "whatsapp_onboarding"
}
```

**Response (201):**
```json
{
  "data": {
    "user_id": "user-uuid",
    "tenant_id": "tenant-uuid",
    "magic_link": "https://app.astrafuture.app/auth?token=xyz123",
    "expires_in": 600
  }
}
```

---

### Passo 9: Enviar Magic Link

**Node:** Send WhatsApp Message

**Mensagem:**
```
🎉 *Sua conta foi criada com sucesso!*

Clique no link abaixo para acessar seu dashboard:

👉 https://app.astrafuture.app/auth?token=xyz123

⏰ Este link expira em 10 minutos.

Ao entrar, você poderá:
✅ Configurar seus horários de atendimento
✅ Adicionar sua equipe
✅ Receber seu primeiro agendamento

*Dica:* Salve este número para suporte rápido! 💬
```

---

### Passo 10: Setup Assistido no Dashboard

Quando usuário clica no Magic Link:

**Backend:**
1. Valida token
2. Cria sessão (JWT)
3. Redireciona para `/onboarding/welcome`

**Frontend (Next.js):**

#### Screen 1: Welcome
```tsx
<OnboardingScreen>
  <h1>Bem-vindo ao AstraFuture! 🚀</h1>
  <p>Vamos configurar tudo em 3 passos rápidos:</p>
  
  <ProgressBar steps={3} current={1} />
  
  <Steps>
    <Step icon="⏰" title="Seus horários de atendimento" />
    <Step icon="👥" title="Convidar equipe (opcional)" />
    <Step icon="✨" title="Personalizar visual" />
  </Steps>
  
  <Button>Começar Setup</Button>
</OnboardingScreen>
```

#### Screen 2: Working Hours (Smart)
```tsx
<OnboardingScreen>
  <h2>⏰ Quando você atende?</h2>
  
  {/* Atalhos Inteligentes */}
  <QuickPresets>
    <Preset 
      label="Seg-Sex, 9h-18h" 
      onClick={() => applyPreset('business_hours')}
    />
    <Preset 
      label="Seg-Sex, 8h-17h (1h almoço)" 
      onClick={() => applyPreset('business_hours_lunch')}
    />
    <Preset 
      label="Personalizar" 
      onClick={() => showCustomEditor()}
    />
  </QuickPresets>
  
  <Button>Continuar</Button>
  <Button variant="ghost">Configurar depois</Button>
</OnboardingScreen>
```

#### Screen 3: Team (Opcional)
```tsx
<OnboardingScreen>
  <h2>👥 Tem uma equipe?</h2>
  <p>Convide colaboradores para acessar a plataforma:</p>
  
  <EmailInput 
    placeholder="maria@clinica.com.br"
    onAdd={sendInvite}
  />
  
  <InvitedList>
    {/* Lista de convites enviados */}
  </InvitedList>
  
  <Button>Continuar</Button>
  <Button variant="ghost">Pular esta etapa</Button>
</OnboardingScreen>
```

#### Screen 4: Branding
```tsx
<OnboardingScreen>
  <h2>✨ Personalize sua marca</h2>
  
  <LogoUpload 
    label="Logo da sua empresa"
    onUpload={handleLogoUpload}
  />
  
  <ColorPicker 
    label="Cor principal"
    defaultColor="#3B82F6"
    onChange={handleColorChange}
  />
  
  <Preview>
    {/* Preview do dashboard com branding aplicado */}
  </Preview>
  
  <Button>Finalizar Setup</Button>
</OnboardingScreen>
```

---

### Passo 11: Onboarding Completo

**Backend:** Atualizar `tenants.onboarding_completed_at`

**Frontend:** Redirecionar para Dashboard Principal

**Mensagem Final (WhatsApp):**
```
✅ *Setup concluído com sucesso!*

Seu dashboard está pronto: https://app.astrafuture.app

*Primeiros passos:*
1️⃣ Adicione seus primeiros clientes
2️⃣ Configure tipos de serviço
3️⃣ Compartilhe seu link de agendamento

Precisa de ajuda? Responda aqui! 💬
```

---

## Flow Alternativo: Usuário Existente

**Trigger:** Customer já existe no sistema

**Mensagem:**
```
Olá novamente, Dr. João! 👋

Já temos seu cadastro no sistema.

Quer acessar seu dashboard?

1️⃣ Sim, enviar link de acesso
2️⃣ Agendar um horário
3️⃣ Falar com suporte

Envie o número da opção!
```

---

## Configuração n8n (JSON)

```json
{
  "name": "WhatsApp Onboarding Flow",
  "nodes": [
    {
      "parameters": {
        "httpMethod": "POST",
        "path": "webhook-whatsapp",
        "responseMode": "onReceived",
        "options": {}
      },
      "name": "Webhook - Evolution API",
      "type": "n8n-nodes-base.webhook",
      "position": [250, 300]
    },
    {
      "parameters": {
        "url": "https://api.pilar1.app/api/v1/customers/check-phone",
        "authentication": "predefinedCredentialType",
        "nodeCredentialType": "pilar1Api",
        "sendQuery": true,
        "queryParameters": {
          "parameters": [
            {
              "name": "phone",
              "value": "={{ $json.data.key.remoteJid.split('@')[0] }}"
            }
          ]
        }
      },
      "name": "Check Existing Customer",
      "type": "n8n-nodes-base.httpRequest",
      "position": [450, 300]
    },
    {
      "parameters": {
        "conditions": {
          "boolean": [
            {
              "value1": "={{ $json.exists }}",
              "value2": false
            }
          ]
        }
      },
      "name": "Is New Lead?",
      "type": "n8n-nodes-base.if",
      "position": [650, 300]
    },
    {
      "parameters": {
        "method": "POST",
        "url": "https://evolution-api.pilar1.app/message/sendText",
        "authentication": "predefinedCredentialType",
        "nodeCredentialType": "evolutionApi",
        "sendBody": true,
        "bodyParameters": {
          "parameters": [
            {
              "name": "number",
              "value": "={{ $('Webhook - Evolution API').item.json.data.key.remoteJid }}"
            },
            {
              "name": "text",
              "value": "👋 Olá! Bem-vindo ao Pilar 1...\n\nQual o nome da sua empresa?"
            }
          ]
        }
      },
      "name": "Send Welcome Message",
      "type": "n8n-nodes-base.httpRequest",
      "position": [850, 200]
    }
    // ... (continua com demais nodes)
  ],
  "connections": {
    "Webhook - Evolution API": {
      "main": [[{"node": "Check Existing Customer"}]]
    },
    "Check Existing Customer": {
      "main": [[{"node": "Is New Lead?"}]]
    }
    // ... (continua)
  }
}
```

---

## Métricas de Sucesso do Onboarding

| Métrica | Target | Medição |
|---------|--------|---------|
| **Time to First Login** | < 3 min | Timestamp(magic_link_click) - Timestamp(first_message) |
| **Completion Rate** | > 80% | % que completa todos os 4 screens |
| **Drop-off Point** | Identificar | Analytics em cada step |
| **Time to First Appointment** | < 24h | Timestamp(first_appointment) - Timestamp(account_created) |

---

## Fallback & Error Handling

### Timeout (usuário não responde)
**Após 10 minutos de inatividade:**
```
Ainda aí? 😊

Se quiser continuar depois, é só mandar um "Oi" novamente!

Seu progresso foi salvo.
```

### Token Expirado
**Se usuário clica em Magic Link após 10 min:**
```
Ops! Este link expirou 😅

Sem problema! Vou te enviar um novo:

👉 https://app.pilar1.app/auth?token=novo-token-123
```

### Erro na Criação da Conta
```
❌ Ops! Algo deu errado ao criar sua conta.

Nossa equipe foi notificada e vai resolver rapidinho!

Enquanto isso, pode me enviar um email em contato@pilar1.app? Vou priorizar seu cadastro! 🚀
```

---

## Integrações n8n

### Evolution API (WhatsApp)
**Webhook URL:** `https://n8n.astrafuture.app/webhook/whatsapp`  
**Events:**
- `messages.upsert` (nova mensagem)
- `messages.update` (status de entrega)

### AstraFuture Backend API
**Base URL:** `https://api.astrafuture.app/api/v1`  
**Authentication:** API Key (n8n Credentials)

### OpenAI (Classificação)
**Model:** `gpt-4o-mini`  
**Max Tokens:** 50  
**Temperature:** 0.3 (mais determinístico)

---

## Otimizações Futuras

### V2: Voice Input
- Permitir envio de áudio via WhatsApp
- Transcrever com Whisper API
- Extrair informações com GPT-4

### V3: Document Upload
- "Envie uma foto do seu cartão CNPJ"
- OCR + validação automática
- Preencher dados automaticamente

### V4: Agendamento Direto
- "Quer agendar sua primeira consulta agora?"
- Calendar inline no WhatsApp
- Zero saída do chat

---

## Arquivos Exportados

- [n8n-workflow.json](./n8n-whatsapp-onboarding.json) - Workflow completo para importar no n8n
- [evolution-api-config.json](./evolution-api-config.json) - Configuração da Evolution API

---

**Próximo:** [UX Strategy Premium](../docs/ux-strategy.md)
