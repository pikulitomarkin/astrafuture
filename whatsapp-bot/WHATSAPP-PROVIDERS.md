# 🤖 Guia de Configuração - WhatsApp Providers

## Opção 1: Twilio (API Oficial WhatsApp)

### Vantagens
- ✅ API oficial do WhatsApp
- ✅ Suporte profissional
- ✅ Alta confiabilidade
- ✅ Compliance total

### Desvantagens
- ❌ Custos por mensagem
- ❌ Processo de aprovação do número
- ❌ Limitações de template (mensagens proativas)

### Configuração

1. **Criar conta Twilio**
   - Acesse: https://www.twilio.com/console
   - Crie uma conta (trial ou paid)

2. **Configurar WhatsApp Sandbox (teste)**
   - Console → Messaging → Try it Out → Send a WhatsApp message
   - Escaneie QR Code com WhatsApp
   - Envie mensagem "join [seu-código]"

3. **Variáveis de Ambiente**
```bash
WHATSAPP_PROVIDER=twilio
TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=your_auth_token
TWILIO_WHATSAPP_NUMBER=whatsapp:+14155238886
```

4. **Configurar Webhook**
   - Console → Messaging → Settings → WhatsApp Sandbox Settings
   - WHEN A MESSAGE COMES IN: `https://seu-dominio.com/webhook`
   - Método: HTTP POST

5. **Para Produção (número próprio)**
   - Solicitar WhatsApp Business API
   - Processo de aprovação (~1-2 semanas)
   - Configurar templates de mensagens

---

## Opção 2: Evolution API (Open Source)

### Vantagens
- ✅ 100% gratuito
- ✅ Sem custos por mensagem
- ✅ Sem limitações de template
- ✅ Mais flexível
- ✅ Self-hosted ou cloud

### Desvantagens
- ❌ Requer servidor próprio
- ❌ Risco de ban (uso não oficial)
- ❌ Manutenção manual

### Configuração

1. **Instalar Evolution API**

   **Opção A: Docker (recomendado)**
   ```bash
   docker run -d \
     --name evolution-api \
     -p 8080:8080 \
     -e AUTHENTICATION_API_KEY=seu-api-key-secreto \
     atendai/evolution-api:latest
   ```

   **Opção B: Docker Compose**
   ```yaml
   version: '3.8'
   services:
     evolution-api:
       image: atendai/evolution-api:latest
       ports:
         - "8080:8080"
       environment:
         - AUTHENTICATION_API_KEY=seu-api-key-secreto
         - DATABASE_ENABLED=true
         - DATABASE_CONNECTION_URI=mongodb://mongo:27017/evolution
       depends_on:
         - mongo
     
     mongo:
       image: mongo:latest
       ports:
         - "27017:27017"
       volumes:
         - evolution_data:/data/db

   volumes:
     evolution_data:
   ```

2. **Criar Instância WhatsApp**
   ```bash
   curl -X POST http://localhost:8080/instance/create \
     -H "Content-Type: application/json" \
     -H "apikey: seu-api-key-secreto" \
     -d '{
       "instanceName": "astra-agenda",
       "qrcode": true,
       "integration": "WHATSAPP-BAILEYS"
     }'
   ```

3. **Conectar WhatsApp**
   - Acesse: `http://localhost:8080/instance/connect/astra-agenda`
   - Escaneie QR Code com WhatsApp
   - Aguarde conexão

4. **Variáveis de Ambiente**
```bash
WHATSAPP_PROVIDER=evolution
EVOLUTION_API_URL=http://localhost:8080
EVOLUTION_API_KEY=seu-api-key-secreto
EVOLUTION_INSTANCE_NAME=astra-agenda
```

5. **Configurar Webhook**
   ```bash
   curl -X POST http://localhost:8080/webhook/set/astra-agenda \
     -H "Content-Type: application/json" \
     -H "apikey: seu-api-key-secreto" \
     -d '{
       "url": "https://seu-dominio.com/webhook",
       "webhook_by_events": false,
       "webhook_base64": false,
       "events": [
         "MESSAGES_UPSERT",
         "MESSAGES_UPDATE"
       ]
     }'
   ```

6. **Deploy no Railway (Evolution API)**
   - Fork do repositório: https://github.com/EvolutionAPI/evolution-api
   - Deploy no Railway
   - Configurar variáveis de ambiente
   - Usar URL gerada

---

## Comparação Rápida

| Recurso | Twilio | Evolution API |
|---------|--------|---------------|
| **Custo** | Pago | Grátis |
| **Setup** | Fácil | Médio |
| **Oficial** | ✅ | ❌ |
| **Templates** | Obrigatório | Não |
| **Manutenção** | Baixa | Média |
| **Escalabilidade** | Alta | Média |
| **Risco Ban** | Nenhum | Baixo-Médio |

---

## Qual Escolher?

### Use **Twilio** se:
- Projeto comercial/empresarial
- Precisa de compliance
- Orçamento disponível
- Quer suporte oficial

### Use **Evolution API** se:
- Projeto pessoal/MVP
- Orçamento limitado
- Precisa de flexibilidade
- Aceita gerenciar infraestrutura

---

## Testando o Bot

### Twilio
```bash
# Enviar mensagem de teste
curl -X POST https://seu-bot.railway.app/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "+5511999999999",
    "message": "Olá! Bot funcionando via Twilio"
  }'
```

### Evolution API
```bash
# Enviar mensagem de teste
curl -X POST https://seu-bot.railway.app/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "5511999999999",
    "message": "Olá! Bot funcionando via Evolution API"
  }'
```

---

## Troubleshooting

### Twilio
- **Webhook não funciona**: Verificar URL pública e HTTPS
- **Mensagens não enviam**: Verificar saldo da conta
- **401 Unauthorized**: Verificar Account SID e Auth Token

### Evolution API
- **QR Code não aparece**: Verificar logs do container
- **Desconecta**: Reiniciar instância ou reescanear QR
- **Webhook não chama**: Verificar URL e eventos configurados
- **500 Error**: Verificar se MongoDB está rodando
