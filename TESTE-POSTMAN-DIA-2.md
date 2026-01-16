# 🚀 AstraFuture API - Teste Postman (Dia 2)

## ✅ API Rodando

**URL Base:** `http://localhost:5000`

**Health Check:**
```
GET http://localhost:5000/health
```

---

## 📋 Endpoint: Criar Appointment

### POST /api/appointments

**URL Completa:**
```
POST http://localhost:5000/api/appointments
```

### Headers:
```
Content-Type: application/json
```

### Body (JSON):
```json
{
  "tenantId": "00000000-0000-0000-0000-000000000001",
  "customerId": "00000000-0000-0000-0000-000000000004",
  "resourceId": "00000000-0000-0000-0000-000000000003",
  "title": "Consulta Teste - Postman",
  "description": "Teste de criação via API",
  "scheduledAt": "2026-01-17T14:00:00Z",
  "durationMinutes": 60,
  "appointmentType": "consultation",
  "notes": "Primeira consulta criada via backend"
}
```

### Resposta Esperada (201 Created):
```json
{
  "id": "uuid-do-appointment-criado"
}
```

---

## 🧪 Passo a Passo no Postman:

1. **Abra Postman**
2. **Crie nova request**:
   - Método: `POST`
   - URL: `http://localhost:5000/api/appointments`

3. **Aba Headers**:
   - Key: `Content-Type`
   - Value: `application/json`

4. **Aba Body**:
   - Selecione `raw`
   - Formato: `JSON`
   - Cole o JSON acima

5. **Click em SEND**

6. **Verifique**:
   - Status: `201 Created`
   - Response tem ID do appointment

---

## ✅ Validação Supabase:

Após criar via Postman, confirme no SQL Editor:

```sql
SELECT * FROM appointments
ORDER BY created_at DESC
LIMIT 5;
```

Deve aparecer o appointment "Consulta Teste - Postman"!

---

## 🎯 Dia 2 Completo!

✅ Infrastructure Layer (SupabaseContext, Repositories, UnitOfWork)  
✅ Application Layer (CreateAppointmentCommand + Handler + Validator)  
✅ API Controller (POST /appointments)  
✅ Dependency Injection configurado  
✅ API rodando em http://localhost:5000  
✅ Criar appointment funciona via Postman  

**Próximo:** Dia 3 - CRUD completo + Autenticação JWT
