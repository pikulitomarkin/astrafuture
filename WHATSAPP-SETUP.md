# 📱 WhatsApp Integration - Python + Evolution API

**Objetivo:** Criar um bot WhatsApp para onboarding de clientes e gestão de agendamentos.

**Stack:**
- **Evolution API** - API WhatsApp (auto-hospedável)
- **Python** - Bot logic e flow builder
- **FastAPI** - Webhook receiver
- **Backend .NET** - Integração com sistema principal

---

## 🎯 Funcionalidades do Bot

### Fase 1 - Onboarding (Dia 9 Manhã)
```
Cliente: "Oi"
Bot: "Olá! Bem-vindo ao [Nome da Empresa]! 👋"
Bot: "Sou o assistente virtual. Como posso te ajudar?"
Bot: "1️⃣ Agendar consulta"
Bot: "2️⃣ Ver meus agendamentos"
Bot: "3️⃣ Falar com atendente"

Cliente: "1"
Bot: "Ótimo! Para qual dia você gostaria de agendar?"
Cliente: "Amanhã às 14h"
Bot: "Perfeito! Confirma o agendamento para [DATA] às 14:00?"
Cliente: "Sim"
Bot: "✅ Agendamento confirmado!"
Bot: "Você receberá um lembrete 1 hora antes."
```

### Fase 2 - Gestão (Dia 9 Tarde)
- Ver agendamentos existentes
- Remarcar agendamento
- Cancelar agendamento
- Confirmação automática (1h antes)
- Pesquisa de satisfação (após atendimento)

---

## 🛠️ Arquitetura

```
┌─────────────────────────────────────┐
│         WhatsApp (Cliente)          │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│         Evolution API                │
│   (Gerencia conexão WhatsApp)       │
└──────────────┬──────────────────────┘
               │ Webhook
               ▼
┌─────────────────────────────────────┐
│      Python Bot (FastAPI)            │
│  - Processa mensagens                │
│  - Flow builder                      │
│  - NLP simples                       │
└──────────────┬──────────────────────┘
               │ REST API
               ▼
┌─────────────────────────────────────┐
│      Backend .NET                    │
│  - Criar/ler agendamentos            │
│  - Gerenciar clientes                │
│  - Validar disponibilidade           │
└─────────────────────────────────────┘
```

---

## 📦 Setup - Evolution API

### Passo 1: Instalar Evolution API

**Opção 1: Docker (Recomendado)**

```bash
# Criar pasta
mkdir whatsapp-bot
cd whatsapp-bot

# Criar docker-compose.yml
cat > docker-compose.yml << 'EOF'
version: '3'

services:
  evolution-api:
    image: atendai/evolution-api:latest
    container_name: evolution-api
    ports:
      - "8080:8080"
    environment:
      - SERVER_URL=http://localhost:8080
      - AUTHENTICATION_API_KEY=sua_chave_secreta_aqui
      - DATABASE_ENABLED=true
      - DATABASE_CONNECTION_URI=mongodb://mongo:27017/evolution
    depends_on:
      - mongo
    restart: unless-stopped

  mongo:
    image: mongo:latest
    container_name: mongo-evolution
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db
    restart: unless-stopped

volumes:
  mongo_data:
EOF

# Iniciar
docker-compose up -d
```

**Opção 2: NPM**

```bash
npm install -g @evolution-api/evolution-api
evolution-api start
```

### Passo 2: Criar Instância WhatsApp

```bash
# POST http://localhost:8080/instance/create
curl -X POST http://localhost:8080/instance/create \
  -H "Content-Type: application/json" \
  -H "apikey: sua_chave_secreta_aqui" \
  -d '{
    "instanceName": "astrafuture-bot",
    "qrcode": true,
    "webhookUrl": "http://seu-servidor/webhook",
    "webhookEvents": ["MESSAGES_UPSERT"]
  }'
```

### Passo 3: Conectar WhatsApp

1. Acesse: `http://localhost:8080/instance/connect/astrafuture-bot`
2. Escanear QR Code com WhatsApp
3. ✅ Conectado!

---

## 🐍 Setup - Bot Python

### Estrutura do Projeto

```
whatsapp-bot/
├── app/
│   ├── __init__.py
│   ├── main.py              # FastAPI server
│   ├── bot/
│   │   ├── __init__.py
│   │   ├── flow_builder.py  # Lógica do bot
│   │   ├── nlp.py          # Processamento de linguagem
│   │   └── handlers.py     # Handlers de mensagens
│   ├── services/
│   │   ├── __init__.py
│   │   ├── evolution.py    # Cliente Evolution API
│   │   └── backend.py      # Cliente Backend .NET
│   └── models/
│       ├── __init__.py
│       └── schemas.py      # Pydantic models
├── requirements.txt
├── .env
└── docker-compose.yml
```

### requirements.txt

```txt
fastapi==0.109.0
uvicorn[standard]==0.27.0
httpx==0.26.0
python-dotenv==1.0.0
pydantic==2.5.3
pydantic-settings==2.1.0
python-multipart==0.0.6
redis==5.0.1
```

### .env

```bash
# Evolution API
EVOLUTION_API_URL=http://localhost:8080
EVOLUTION_API_KEY=sua_chave_secreta_aqui
EVOLUTION_INSTANCE=astrafuture-bot

# Backend .NET
BACKEND_API_URL=https://seu-app.up.railway.app/api

# Bot Config
BOT_NAME=AstraBot
BOT_WEBHOOK_PORT=8000
```

### main.py

```python
from fastapi import FastAPI, Request, BackgroundTasks
from fastapi.responses import JSONResponse
import httpx
from dotenv import load_dotenv
import os
from app.bot.flow_builder import BotFlowBuilder
from app.services.evolution import EvolutionService
from app.services.backend import BackendService

load_dotenv()

app = FastAPI(title="AstraFuture WhatsApp Bot")

evolution = EvolutionService(
    base_url=os.getenv("EVOLUTION_API_URL"),
    api_key=os.getenv("EVOLUTION_API_KEY"),
    instance=os.getenv("EVOLUTION_INSTANCE")
)

backend = BackendService(
    base_url=os.getenv("BACKEND_API_URL")
)

bot = BotFlowBuilder(evolution, backend)

@app.post("/webhook")
async def webhook(request: Request, background_tasks: BackgroundTasks):
    """Recebe mensagens do Evolution API"""
    data = await request.json()
    
    # Processar em background para responder rápido
    background_tasks.add_task(process_message, data)
    
    return JSONResponse({"status": "received"})

async def process_message(data: dict):
    """Processa mensagem recebida"""
    try:
        # Extrair informações
        message = data.get("data", {})
        key = message.get("key", {})
        
        from_number = key.get("remoteJid", "").replace("@s.whatsapp.net", "")
        message_text = message.get("message", {}).get("conversation", "")
        
        if not message_text:
            return
        
        # Processar com o bot
        response = await bot.process_message(from_number, message_text)
        
        # Enviar resposta
        if response:
            await evolution.send_message(from_number, response)
            
    except Exception as e:
        print(f"Error processing message: {e}")

@app.get("/health")
async def health():
    return {"status": "healthy", "bot": os.getenv("BOT_NAME")}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=int(os.getenv("BOT_WEBHOOK_PORT", 8000)))
```

### app/bot/flow_builder.py

```python
from typing import Optional
import re
from datetime import datetime, timedelta

class BotFlowBuilder:
    def __init__(self, evolution_service, backend_service):
        self.evolution = evolution_service
        self.backend = backend_service
        self.user_states = {}  # Armazenar estado de cada usuário
        
    async def process_message(self, phone: str, message: str) -> Optional[str]:
        """Processa mensagem e retorna resposta"""
        
        # Normalizar mensagem
        message = message.lower().strip()
        
        # Obter estado atual do usuário
        state = self.user_states.get(phone, {"step": "initial"})
        
        # Flow de conversação
        if state["step"] == "initial":
            return await self.handle_initial(phone, message, state)
        
        elif state["step"] == "menu":
            return await self.handle_menu(phone, message, state)
        
        elif state["step"] == "scheduling_date":
            return await self.handle_scheduling_date(phone, message, state)
        
        elif state["step"] == "scheduling_time":
            return await self.handle_scheduling_time(phone, message, state)
        
        elif state["step"] == "scheduling_confirm":
            return await self.handle_scheduling_confirm(phone, message, state)
        
        return "Desculpe, não entendi. Digite 'menu' para ver as opções."
    
    async def handle_initial(self, phone: str, message: str, state: dict) -> str:
        """Mensagem inicial"""
        self.user_states[phone] = {"step": "menu"}
        
        return (
            "Olá! 👋 Bem-vindo ao *AstraFuture*!\n\n"
            "Sou seu assistente virtual. Como posso ajudar?\n\n"
            "1️⃣ Agendar consulta\n"
            "2️⃣ Ver meus agendamentos\n"
            "3️⃣ Falar com atendente\n\n"
            "_Digite o número da opção desejada_"
        )
    
    async def handle_menu(self, phone: str, message: str, state: dict) -> str:
        """Menu principal"""
        if message in ["1", "agendar", "marcar"]:
            self.user_states[phone] = {"step": "scheduling_date"}
            return (
                "Ótimo! Vamos agendar sua consulta. 📅\n\n"
                "Para qual dia você gostaria?\n\n"
                "_Exemplos:_\n"
                "- Amanhã\n"
                "- 25/01\n"
                "- Segunda-feira"
            )
        
        elif message in ["2", "ver", "agendamentos"]:
            # Buscar agendamentos do backend
            appointments = await self.backend.get_appointments_by_phone(phone)
            
            if not appointments:
                return "Você não tem agendamentos. Gostaria de agendar? (Digite 1)"
            
            response = "📋 *Seus agendamentos:*\n\n"
            for apt in appointments:
                response += f"• {apt['date']} às {apt['time']}\n"
                response += f"  Status: {apt['status']}\n\n"
            
            return response
        
        elif message in ["3", "atendente", "humano"]:
            return (
                "Vou transferir você para um atendente humano.\n"
                "Aguarde um momento... ⏳"
            )
        
        else:
            return (
                "Opção inválida. Digite:\n\n"
                "1 - Agendar\n"
                "2 - Ver agendamentos\n"
                "3 - Atendente"
            )
    
    async def handle_scheduling_date(self, phone: str, message: str, state: dict) -> str:
        """Processar data do agendamento"""
        date = self.parse_date(message)
        
        if not date:
            return (
                "Não entendi a data. Pode tentar novamente?\n\n"
                "Exemplos: 'amanhã', '25/01', 'segunda-feira'"
            )
        
        # Verificar disponibilidade
        available_times = await self.backend.get_available_times(date)
        
        if not available_times:
            return f"Não temos horários disponíveis para {date}. Tente outra data."
        
        self.user_states[phone] = {
            "step": "scheduling_time",
            "date": date,
            "available_times": available_times
        }
        
        times_text = "\n".join([f"• {time}" for time in available_times[:5]])
        
        return (
            f"Ótimo! Para {date} temos os seguintes horários:\n\n"
            f"{times_text}\n\n"
            "Qual horário prefere?"
        )
    
    async def handle_scheduling_time(self, phone: str, message: str, state: dict) -> str:
        """Processar horário do agendamento"""
        time = self.parse_time(message)
        
        if not time or time not in state["available_times"]:
            return "Horário inválido. Escolha um dos horários disponíveis."
        
        self.user_states[phone] = {
            "step": "scheduling_confirm",
            "date": state["date"],
            "time": time
        }
        
        return (
            f"✅ Resumo do agendamento:\n\n"
            f"📅 Data: {state['date']}\n"
            f"🕐 Horário: {time}\n\n"
            "Confirma? (sim/não)"
        )
    
    async def handle_scheduling_confirm(self, phone: str, message: str, state: dict) -> str:
        """Confirmar agendamento"""
        if message in ["sim", "s", "confirmo", "ok"]:
            # Criar agendamento no backend
            appointment = await self.backend.create_appointment(
                phone=phone,
                date=state["date"],
                time=state["time"]
            )
            
            # Resetar estado
            self.user_states[phone] = {"step": "initial"}
            
            return (
                "🎉 *Agendamento confirmado!*\n\n"
                f"📅 {state['date']} às {state['time']}\n\n"
                "Você receberá um lembrete 1 hora antes.\n"
                "Até lá! 👋"
            )
        
        else:
            self.user_states[phone] = {"step": "menu"}
            return "Agendamento cancelado. Digite 'menu' para ver as opções."
    
    def parse_date(self, message: str) -> Optional[str]:
        """Parseia data da mensagem"""
        today = datetime.now()
        
        if "amanhã" in message or "amanha" in message:
            return (today + timedelta(days=1)).strftime("%d/%m/%Y")
        
        # Tentar formato DD/MM
        match = re.search(r'(\d{1,2})/(\d{1,2})', message)
        if match:
            day, month = match.groups()
            return f"{day.zfill(2)}/{month.zfill(2)}/{today.year}"
        
        # TODO: Adicionar mais lógica (dia da semana, etc)
        
        return None
    
    def parse_time(self, message: str) -> Optional[str]:
        """Parseia horário da mensagem"""
        # Tentar formato HH:MM ou HHh
        match = re.search(r'(\d{1,2}):?(\d{2})?', message)
        if match:
            hour = match.group(1)
            minute = match.group(2) or "00"
            return f"{hour.zfill(2)}:{minute}"
        
        return None
```

### app/services/evolution.py

```python
import httpx
from typing import Optional

class EvolutionService:
    def __init__(self, base_url: str, api_key: str, instance: str):
        self.base_url = base_url
        self.api_key = api_key
        self.instance = instance
        self.headers = {"apikey": api_key}
    
    async def send_message(self, phone: str, text: str) -> bool:
        """Envia mensagem via Evolution API"""
        url = f"{self.base_url}/message/sendText/{self.instance}"
        
        payload = {
            "number": f"{phone}@s.whatsapp.net",
            "text": text
        }
        
        async with httpx.AsyncClient() as client:
            response = await client.post(url, json=payload, headers=self.headers)
            return response.status_code == 200
    
    async def get_instance_status(self) -> dict:
        """Verifica status da instância"""
        url = f"{self.base_url}/instance/connectionState/{self.instance}"
        
        async with httpx.AsyncClient() as client:
            response = await client.get(url, headers=self.headers)
            return response.json()
```

### app/services/backend.py

```python
import httpx
from typing import List, Optional

class BackendService:
    def __init__(self, base_url: str):
        self.base_url = base_url
    
    async def get_appointments_by_phone(self, phone: str) -> List[dict]:
        """Busca agendamentos por telefone"""
        url = f"{self.base_url}/appointments/by-phone/{phone}"
        
        async with httpx.AsyncClient() as client:
            response = await client.get(url)
            if response.status_code == 200:
                return response.json()
            return []
    
    async def get_available_times(self, date: str) -> List[str]:
        """Busca horários disponíveis"""
        url = f"{self.base_url}/appointments/available"
        params = {"date": date}
        
        async with httpx.AsyncClient() as client:
            response = await client.get(url, params=params)
            if response.status_code == 200:
                return response.json()
            return []
    
    async def create_appointment(self, phone: str, date: str, time: str) -> dict:
        """Cria agendamento"""
        url = f"{self.base_url}/appointments"
        
        payload = {
            "customerPhone": phone,
            "date": date,
            "time": time,
            "source": "whatsapp"
        }
        
        async with httpx.AsyncClient() as client:
            response = await client.post(url, json=payload)
            return response.json()
```

---

## 🚀 Deploy

### Opção 1: Railway (Recomendado)

```bash
# Criar projeto Python
cd whatsapp-bot

# Criar Procfile
echo "web: uvicorn app.main:app --host 0.0.0.0 --port \$PORT" > Procfile

# Criar runtime.txt
echo "python-3.11" > runtime.txt

# Deploy
railway up
```

### Opção 2: Docker

```bash
# Criar Dockerfile
cat > Dockerfile << 'EOF'
FROM python:3.11-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
EOF

# Build
docker build -t whatsapp-bot .

# Run
docker run -p 8000:8000 --env-file .env whatsapp-bot
```

---

## 🧪 Testes

### Testar Webhook

```bash
# Simular mensagem recebida
curl -X POST http://localhost:8000/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "data": {
      "key": {
        "remoteJid": "5511999999999@s.whatsapp.net"
      },
      "message": {
        "conversation": "Oi"
      }
    }
  }'
```

### Testar Bot Completo

1. Envie "Oi" para o número conectado
2. Bot deve responder com menu
3. Digite "1" para agendar
4. Siga o fluxo completo

---

## 📊 Monitoramento

### Logs do Bot

```python
# Adicionar logging em main.py
import logging

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

logger = logging.getLogger(__name__)

# Usar nos handlers
logger.info(f"Message from {from_number}: {message_text}")
logger.info(f"Bot response: {response}")
```

### Dashboard Evolution API

Acesse: `http://localhost:8080/manager`

- Ver instâncias conectadas
- Logs de mensagens
- Estatísticas

---

## 🎯 Roadmap de Melhorias

### Fase 2 (Semana 3)
- [ ] NLP com OpenAI/Anthropic para entender linguagem natural
- [ ] Envio de imagens (comprovantes, localizações)
- [ ] Grupos WhatsApp (alertas coletivos)
- [ ] Integração com calendário Google

### Fase 3 (Semana 4)
- [ ] Chatbot com IA conversacional
- [ ] Pagamentos via WhatsApp
- [ ] Notificações proativas
- [ ] Analytics de conversas

---

**Tempo estimado de implementação:** 1 dia (Dia 9)  
**Última atualização:** 16 Janeiro 2026
