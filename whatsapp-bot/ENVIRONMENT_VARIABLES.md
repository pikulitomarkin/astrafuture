# Variáveis de Ambiente — WhatsApp Bot

Este documento lista todas as variáveis de ambiente necessárias e opcionais para rodar o bot WhatsApp (Astra Agenda), com descrição, exemplo e indicação se é sensível/obrigatória.

> Marque como *secret* as credenciais e tokens antes de adicioná-las ao Railway ou outro provedor.

## Lista de variáveis

- `API_BASE_URL` (Obrigatório)
  - Descrição: URL base da API do Astra Agenda que o bot usa (ex: endpoints `/customers`, `/appointments`).
  - Exemplo: `https://minha-agenda.up.railway.app/api`

- `API_KEY` (Obrigatório, Secret)
  - Descrição: Chave usada no header `X-API-Key` para autenticar requisições ao backend.
  - Exemplo: `sua-api-key-aqui`

- `WHATSAPP_PROVIDER` (Obrigatório)
  - Descrição: Provedor de WhatsApp utilizado pelo bot. Valores: `twilio` ou `evolution`.
  - Exemplo: `twilio`

### Se `WHATSAPP_PROVIDER=twilio`
- `TWILIO_ACCOUNT_SID` (Obrigatório se usar Twilio, Secret)
  - Exemplo: `ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx`
- `TWILIO_AUTH_TOKEN` (Obrigatório se usar Twilio, Secret)
  - Exemplo: `your_auth_token`
- `TWILIO_WHATSAPP_NUMBER` (Obrigatório se usar Twilio)
  - Exemplo: `whatsapp:+14155238886`

### Se `WHATSAPP_PROVIDER=evolution`
- `EVOLUTION_API_URL` (Obrigatório se usar Evolution)
  - Exemplo: `http://evolution.local:8080`
- `EVOLUTION_API_KEY` (Obrigatório se usar Evolution, Secret)
  - Exemplo: `your_evolution_api_key`
- `EVOLUTION_INSTANCE_NAME` (Obrigatório se usar Evolution)
  - Exemplo: `instance01`

## Configurações de servidor / app

- `PORT` (Opcional)
  - Descrição: Porta em que o servidor Flask/Gunicorn deve escutar. O Dockerfile usa 5000 por padrão.
  - Exemplo: `5000`
- `FLASK_ENV` (Opcional)
  - Descrição: Ambiente do Flask (`production` ou `development`).
  - Exemplo: `production`
- `SECRET_KEY` (Obrigatório, Secret)
  - Descrição: Chave secreta do Flask para sessões e segurança.
  - Exemplo: `change-me`
- `REDIS_URL` (Opcional)
  - Exemplo: `redis://redis:6379`
- `TIMEZONE` (Opcional)
  - Exemplo: `America/Sao_Paulo`
- `LOG_LEVEL` (Opcional)
  - Exemplo: `INFO`

## Exemplo de `.env` (para desenvolvimento)

```env
# API Backend
API_BASE_URL=https://minha-agenda.up.railway.app/api
API_KEY=sua-api-key-aqui

# Provedor WhatsApp
WHATSAPP_PROVIDER=twilio

# Twilio
TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=your_auth_token
TWILIO_WHATSAPP_NUMBER=whatsapp:+14155238886

# Servidor
PORT=5000
FLASK_ENV=production
SECRET_KEY=change-me

# Redis (opcional)
REDIS_URL=redis://localhost:6379

# Timezone / logs
TIMEZONE=America/Sao_Paulo
LOG_LEVEL=INFO
```

## Observações e checklist rápido

- Adicione todas as variáveis necessárias no Railway → Service → Settings → Environment Variables e marque *secret* para credenciais. ✅
- Se estiver usando Twilio: configure o webhook em Twilio para `https://<seu-servico>.up.railway.app/webhook`. ✅
- Se estiver usando Evolution: configure a URL e a instância apropriada. ✅
- Recomendo definir `PORT=5000` no Railway para compatibilidade com o Dockerfile atual, ou atualizar o `Dockerfile` para usar `${PORT}` dinamicamente. 🔧

---

Se quiser, eu atualizo o `.env.example` com esse conteúdo ou adiciono diretamente as variáveis no Railway (preciso de acesso). Quer que eu atualize o `.env.example` também? 🔧