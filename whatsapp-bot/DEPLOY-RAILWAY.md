# 🚂 Deploy Bot WhatsApp no Railway

## Pré-requisitos

- ✅ Conta no Railway (https://railway.app)
- ✅ Repositório GitHub com o código
- ✅ Credenciais do provedor WhatsApp escolhido (Twilio ou Evolution API)
- ✅ API Backend já deployada (para pegar a URL)

---

## Passo 1: Criar Novo Serviço

1. Acesse: https://railway.app/dashboard
2. Clique em **New Project**
3. Selecione **Deploy from GitHub repo**
4. Escolha o repositório: `astrafuture`

---

## Passo 2: Configurar Root Directory

⚠️ **IMPORTANTE**: O Railway precisa saber onde está o bot.

1. No serviço criado, vá em **Settings**
2. **Root Directory** → `whatsapp-bot`
3. **Builder** → Deixe em `Dockerfile` (Railway vai detectar automaticamente)

---

## Passo 3: Configurar Variáveis de Ambiente

No Railway, vá em **Variables** e adicione:

### Variáveis Comuns (Obrigatórias)

```bash
# API Backend
API_BASE_URL=https://seu-backend.up.railway.app/api
API_KEY=sua-api-key-aqui

# Servidor
PORT=5000
FLASK_ENV=production
SECRET_KEY=gere-uma-chave-secreta-forte-aqui

# Logs
LOG_LEVEL=INFO
TIMEZONE=America/Sao_Paulo
```

### Escolha o Provider:

#### Opção A: Twilio (Recomendado para Produção)

```bash
WHATSAPP_PROVIDER=twilio
TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=seu_auth_token_aqui
TWILIO_WHATSAPP_NUMBER=whatsapp:+14155238886
```

#### Opção B: Evolution API (Grátis)

```bash
WHATSAPP_PROVIDER=evolution
EVOLUTION_API_URL=https://sua-evolution-api.com
EVOLUTION_API_KEY=sua_evolution_api_key
EVOLUTION_INSTANCE_NAME=astra-agenda
```

---

## Passo 4: Deploy

1. Clique em **Deploy**
2. Railway vai:
   - Detectar o Dockerfile
   - Instalar dependências Python
   - Buildar a imagem Docker
   - Fazer deploy do bot

3. Aguarde o deploy finalizar (~2-3 minutos)

---

## Passo 5: Obter URL do Bot

Após o deploy:

1. Railway vai gerar uma URL pública: `https://seu-bot-xxxxxx.up.railway.app`
2. **Copie essa URL** - você vai precisar para configurar o webhook

---

## Passo 6: Configurar Webhook

### Se usando Twilio:

1. Acesse: https://console.twilio.com/
2. **Messaging** → **Settings** → **WhatsApp Sandbox Settings**
3. Em **"WHEN A MESSAGE COMES IN"**:
   - URL: `https://seu-bot-xxxxxx.up.railway.app/webhook`
   - Método: `HTTP POST`
4. Salve

### Se usando Evolution API:

Execute este comando (substitua as variáveis):

```bash
curl -X POST https://sua-evolution-api.com/webhook/set/astra-agenda \
  -H "Content-Type: application/json" \
  -H "apikey: sua-api-key" \
  -d '{
    "url": "https://seu-bot-xxxxxx.up.railway.app/webhook",
    "webhook_by_events": false,
    "webhook_base64": false,
    "events": [
      "MESSAGES_UPSERT",
      "MESSAGES_UPDATE"
    ]
  }'
```

---

## Passo 7: Testar o Bot

### 1. Health Check

```bash
curl https://seu-bot-xxxxxx.up.railway.app/health
```

Deve retornar:
```json
{
  "status": "healthy",
  "service": "whatsapp-bot",
  "version": "1.0.0"
}
```

### 2. Teste de Envio Manual

```bash
curl -X POST https://seu-bot-xxxxxx.up.railway.app/send \
  -H "Content-Type: application/json" \
  -d '{
    "to": "+5511999999999",
    "message": "🤖 Bot Astra Agenda Online!"
  }'
```

### 3. Teste via WhatsApp

**Se Twilio Sandbox:**
- Envie "join [seu-código]" para o número do sandbox
- Depois envie: "menu" ou "oi"

**Se Evolution API:**
- Envie "menu" ou "oi" para o número conectado

---

## Estrutura de Arquivos (Railway)

O Railway vai usar esta estrutura:

```
whatsapp-bot/
├── Dockerfile              ← Railway usa este
├── requirements.txt        ← Dependências
├── src/
│   ├── bot.py             ← Aplicação principal
│   ├── config.py          ← Lê variáveis de ambiente
│   ├── handlers/
│   └── services/
└── .env (NÃO incluir)     ← Railway usa Variables
```

---

## Verificar Logs

No Railway:

1. Vá no serviço do bot
2. Clique em **Deployments**
3. Selecione o deployment ativo
4. Veja os logs em tempo real

Logs esperados:
```
🤖 Iniciando WhatsApp Bot na porta 5000
Usando Twilio como provedor WhatsApp
📡 Webhook: http://0.0.0.0:5000/webhook
🏥 Health: http://0.0.0.0:5000/health
```

---

## Troubleshooting

### Erro: "Could not find Dockerfile"
**Solução**: Configure Root Directory para `whatsapp-bot`

### Erro: "Module not found"
**Solução**: Verifique se `requirements.txt` está correto

### Erro: "WHATSAPP_PROVIDER not configured"
**Solução**: Adicione a variável `WHATSAPP_PROVIDER=twilio` ou `WHATSAPP_PROVIDER=evolution`

### Webhook não funciona
**Solução**: 
- Verifique se a URL está correta
- Teste com curl primeiro
- Veja logs do Railway para erros

### Bot não responde
**Soluções**:
- Verifique se API backend está no ar
- Teste endpoint `/health` do bot
- Verifique credenciais do provider (Twilio/Evolution)
- Veja logs para erros de autenticação

---

## Custos Estimados

### Railway (Bot)
- **Starter Plan**: $5/mês
- **Pro Plan**: $20/mês (recomendado)
- Inclui: 500h de runtime, 8GB RAM

### Twilio
- **Sandbox**: Grátis (teste)
- **Produção**: ~$0.005 por mensagem
- Número WhatsApp: $1.50/mês

### Evolution API
- **Self-hosted**: Grátis
- **VPS para hospedar**: $5-10/mês

---

## Próximos Passos

Após o bot estar funcionando:

1. ✅ Configure lembretes automáticos
2. ✅ Adicione mais comandos personalizados
3. ✅ Integre com CRM/sistemas externos
4. ✅ Configure analytics e monitoramento
5. ✅ Implemente testes automatizados

---

## Comandos Úteis

### Ver status do deploy
```bash
# No terminal local
railway status
```

### Ver logs em tempo real
```bash
railway logs
```

### Redeploy manual
```bash
railway up
```

### Conectar ao projeto
```bash
railway link
```

---

## Links Úteis

- Railway Dashboard: https://railway.app/dashboard
- Twilio Console: https://console.twilio.com
- Evolution API Docs: https://doc.evolution-api.com
- Repositório: https://github.com/seu-usuario/astrafuture

---

## Checklist de Deploy

- [ ] Criar serviço no Railway
- [ ] Configurar Root Directory: `whatsapp-bot`
- [ ] Adicionar variáveis de ambiente
- [ ] Aguardar deploy finalizar
- [ ] Copiar URL gerada pelo Railway
- [ ] Configurar webhook no provedor WhatsApp
- [ ] Testar endpoint `/health`
- [ ] Testar envio manual com `/send`
- [ ] Testar conversação via WhatsApp
- [ ] Monitorar logs do Railway

---

🎉 **Bot WhatsApp no ar!** Agora seus clientes podem agendar pelo WhatsApp 24/7!
