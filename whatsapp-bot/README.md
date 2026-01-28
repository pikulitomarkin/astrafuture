# 🤖 WhatsApp Bot - Astra Agenda

Bot Python para processamento automático de agendamentos via WhatsApp.

## Funcionalidades

- ✅ Receber mensagens do WhatsApp via webhook
- ✅ Suporte a **Twilio** (API Oficial WhatsApp)
- ✅ Suporte a **Evolution API** (Open Source)
- ✅ Processar solicitações de agendamento
- ✅ Listar horários disponíveis
- ✅ Confirmar/cancelar agendamentos
- ✅ Enviar lembretes automáticos
- ✅ Integração com API backend
- ✅ Arquitetura flexível com abstração de provedores

## Estrutura

```
whatsapp-bot/
├── src/
│   ├── bot.py              # Bot principal
│   ├── handlers/           # Handlers de mensagens
│   ├── services/           # Serviços (API, WhatsApp)
│   ├── utils/              # Utilitários
│   └── config.py           # Configurações
├── requirements.txt        # Dependências Python
├── .env.example           # Exemplo de variáveis
├── Dockerfile             # Container Docker
└── README.md
```

## Instalação

```bash
cd whatsapp-bot
python -m venv venv
source venv/bin/activate  # Linux/Mac
# ou
venv\Scripts\activate     # Windows

pip install -r requirements.txt
```

## Configuração

1. Copie `.env.example` para `.env`
2. Configure as variáveis de ambiente
3. Execute o bot: `python src/bot.py`

## Deploy

### Railway (Recomendado)

Consulte o guia completo: **[DEPLOY-RAILWAY.md](DEPLOY-RAILWAY.md)**

**Resumo rápido:**
1. Crie novo projeto no Railway
2. Deploy do GitHub: `astrafuture`
3. Settings → Root Directory: `whatsapp-bot`
4. Adicione variáveis de ambiente
5. Configure webhook no provedor WhatsApp

### Docker Local

```bash
cd whatsapp-bot

# Build
docker build -t astra-bot .

# Run
docker run -d \
  --name astra-bot \
  -p 5000:5000 \
  --env-file .env \
  astra-bot
```

### Manual (desenvolvimento)

```bash
cd whatsapp-bot
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate
pip install -r requirements.txt
python src/bot.py
```
