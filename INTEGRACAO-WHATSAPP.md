# 📱 Integração WhatsApp - AstraFuture

## 🚀 Visão Geral

O AstraFuture agora possui uma API completa para integração com WhatsApp e outros serviços externos! Com esta funcionalidade, seus clientes podem:

- ✅ Se cadastrar via WhatsApp
- ✅ Agendar consultas/serviços via WhatsApp
- ✅ Verificar disponibilidade automaticamente
- ✅ Receber confirmações em tempo real

---

## 📋 Passo 1: Executar a Migration

### Opção A: Via Supabase Dashboard (Recomendado)

1. Acesse: https://supabase.com/dashboard/project/alxtzjmtclopraayehfg/sql
2. Copie todo o conteúdo do arquivo `database/migrations/004_whatsapp_integration.sql`
3. Cole no editor SQL
4. Clique em "Run"

### Opção B: Via Linha de Comando (se tiver PostgreSQL instalado)

```powershell
$env:PGPASSWORD='MHd64o*cLZJ@Bv8'
psql -h aws-0-us-west-1.pooler.supabase.com -p 6543 -U postgres.alxtzjmtclopraayehfg -d postgres -f database\migrations\004_whatsapp_integration.sql
```

---

## 🔑 Passo 2: Gerar sua API Key

1. Faça login no AstraFuture
2. Acesse **Dashboard > Integrações**
3. Clique em **"Nova API Key"**
4. Preencha:
   - **Nome**: Ex: "WhatsApp Bot Produção"
   - **Descrição**: Ex: "Bot principal do WhatsApp"
   - **Expiração**: Escolha o período (recomendado: 1 ano)
   - **Rate Limit**: 60 requisições/minuto (recomendado)
5. Clique em **"Criar API Key"**

⚠️ **ATENÇÃO:** Copie a chave imediatamente! Ela só será exibida uma vez.

Exemplo de chave gerada:
```
astrafuture_live_Xy9kLm3nQp8rWv4tUz7aB2cD5eF6gH1iJ0
```

---

## 🌐 Passo 3: Obter as URLs dos Webhooks

Na página **Integrações > Webhook URLs**, você verá:

### 1️⃣ Webhook Principal (Receber Mensagens)
```
POST http://localhost:5045/api/webhook/whatsapp
```
Configure esta URL na Evolution API ou WhatsApp Cloud API

### 2️⃣ Criar Cliente
```
POST http://localhost:5045/api/webhook/customers
Body: {
  "phoneNumber": "5511999999999",
  "name": "João Silva",
  "email": "joao@email.com"
}
```

### 3️⃣ Criar Agendamento
```
POST http://localhost:5045/api/webhook/appointments
Body: {
  "customerPhone": "5511999999999",
  "startTime": "2026-01-30T14:00:00Z",
  "endTime": "2026-01-30T15:00:00Z",
  "notes": "Consulta online"
}
```

### 4️⃣ Verificar se Cliente Existe
```
GET http://localhost:5045/api/webhook/customers/check?phone=5511999999999
```

**IMPORTANTE:** Todas as requisições devem incluir o header:
```
X-API-Key: sua_api_key_aqui
```

---

## 🤖 Passo 4: Configurar Bot do WhatsApp

### Com Evolution API

1. Configure o webhook na Evolution API:
```bash
curl -X POST https://sua-evolution-api.com/instance/set-webhook \
  -H "Content-Type: application/json" \
  -H "apikey: sua_chave_evolution" \
  -d '{
    "webhook": {
      "url": "http://seu-servidor:5045/api/webhook/whatsapp",
      "headers": {
        "X-API-Key": "astrafuture_live_XXXXXXXXXX"
      },
      "events": ["messages.upsert"]
    }
  }'
```

2. Quando uma mensagem chegar, a Evolution API enviará para sua API

### Com n8n (Low-code) - Recomendado para iniciantes

Veja o workflow completo em `workflows/whatsapp-onboarding.md`

---

## 📝 Exemplos de Uso

### Criar Cliente via API

```bash
curl -X POST http://localhost:5045/api/webhook/customers \
  -H "Content-Type: application/json" \
  -H "X-API-Key: astrafuture_live_XXXXXXXXXX" \
  -d '{
    "phoneNumber": "5511987654321",
    "name": "Dr. João Silva",
    "email": "joao@clinica.com"
  }'
```

**Response:**
```json
{
  "success": true,
  "customerId": "uuid-do-cliente",
  "message": "Customer created successfully"
}
```

### Verificar se Cliente Existe

```bash
curl -X GET "http://localhost:5045/api/webhook/customers/check?phone=5511987654321" \
  -H "X-API-Key: astrafuture_live_XXXXXXXXXX"
```

**Response (Cliente existe):**
```json
{
  "exists": true,
  "customer": {
    "id": "uuid",
    "name": "Dr. João Silva",
    "email": "joao@clinica.com",
    "phone": "5511987654321"
  }
}
```

**Response (Cliente não existe):**
```json
{
  "exists": false,
  "customer": null
}
```

### Criar Agendamento

```bash
curl -X POST http://localhost:5045/api/webhook/appointments \
  -H "Content-Type: application/json" \
  -H "X-API-Key: astrafuture_live_XXXXXXXXXX" \
  -d '{
    "customerPhone": "5511987654321",
    "startTime": "2026-01-30T14:00:00Z",
    "endTime": "2026-01-30T15:00:00Z",
    "notes": "Primeira consulta"
  }'
```

**Response (Sucesso):**
```json
{
  "success": true,
  "appointmentId": "uuid-do-agendamento",
  "startTime": "2026-01-30T14:00:00Z",
  "endTime": "2026-01-30T15:00:00Z",
  "message": "Appointment created successfully"
}
```

**Response (Conflito de horário):**
```json
{
  "message": "Time slot already booked"
}
```
Status: 409 Conflict

---

## 🔒 Segurança

### ✅ O que está protegido:

- Todas as requisições exigem API Key válida
- API Keys podem ser desativadas instantaneamente
- Limite de requisições por minuto configurável
- API Keys com data de expiração
- Cada tenant só acessa seus próprios dados

### ⚠️ Boas práticas:

1. **Nunca compartilhe sua API Key publicamente**
2. **Use HTTPS em produção** (nunca HTTP)
3. **Rotacione suas keys periodicamente**
4. **Monitore o uso na dashboard** (campo "Usos")
5. **Desative keys não utilizadas**

---

## 📊 Monitoramento

Na página **Integrações**, você pode:

- Ver todas as suas API Keys
- Verificar quantas vezes cada key foi usada
- Ver quando foi o último uso
- Ativar/Desativar keys
- Deletar keys antigas

Exemplo de informações exibidas:
```
Key: ****xxxxxxxx (últimos 8 caracteres)
Usos: 1,234
Último uso: 27/01/2026
Expira: 27/01/2027
Status: Ativa ✅
```

---

## 🐛 Troubleshooting

### Erro: "API Key is required"
- Verifique se está enviando o header `X-API-Key`
- Certifique-se que o nome está correto (X-API-Key, não X-Api-Key)

### Erro: "Invalid API Key"
- A key pode estar inativa ou expirada
- Verifique na dashboard se a key está ativa
- Gere uma nova key se necessário

### Erro: "Customer not found with this phone number"
- O cliente precisa ser criado antes de agendar
- Use primeiro o endpoint `/api/webhook/customers`
- Depois use `/api/webhook/appointments`

### Erro: "Time slot already booked"
- O horário escolhido já está ocupado
- Verifique horários disponíveis primeiro
- Use outro horário ou recurso

---

## 🚀 Deploy em Produção

Quando fizer deploy, você precisará:

1. **Atualizar as URLs dos webhooks**
   - Substitua `http://localhost:5045` pelo seu domínio
   - Exemplo: `https://api.suaempresa.com`

2. **Usar HTTPS obrigatoriamente**
   - Configure certificado SSL (Let's Encrypt gratuito)
   - WhatsApp Cloud API exige HTTPS

3. **Configurar variáveis de ambiente**
   ```bash
   ASPNETCORE_URLS=https://+:443;http://+:80
   ```

4. **Gerar novas API Keys de produção**
   - Não use as keys de desenvolvimento em produção

---

## 📚 Recursos Adicionais

- [Documentação Evolution API](https://doc.evolution-api.com/)
- [WhatsApp Cloud API](https://developers.facebook.com/docs/whatsapp/cloud-api)
- [n8n Workflows](https://n8n.io/)
- [Workflow completo](workflows/whatsapp-onboarding.md)

---

## 🎯 Próximos Passos Sugeridos

1. ✅ Executar migration (criar tabelas)
2. ✅ Gerar sua primeira API Key
3. ⏳ Configurar Evolution API ou WhatsApp Cloud API
4. ⏳ Testar endpoints com Postman/cURL
5. ⏳ Criar fluxo do bot (n8n ou código)
6. ⏳ Testar com número real de WhatsApp
7. ⏳ Configurar mensagens automáticas
8. ⏳ Deploy em produção

---

## 💡 Dica Final

Para testar rapidamente sem bot:

1. Use Postman ou Insomnia
2. Configure o header `X-API-Key`
3. Teste os endpoints manualmente
4. Depois integre com WhatsApp

**Pronto! Sua API está pronta para receber agendamentos via WhatsApp! 🎉**
