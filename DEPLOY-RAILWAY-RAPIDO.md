# 🚀 Deploy Railway - Guia Rápido

## ⚠️ IMPORTANTE: Configure o Root Directory

O Railway precisa saber qual pasta buildar. Você tem 2 opções:

### Opção 1: Criar 2 Serviços Separados (Recomendado)

#### Serviço 1: Backend
1. No Railway, crie "New Service" → "Deploy from GitHub repo"
2. Escolha o repositório `astrafuture`
3. **Settings** → **Root Directory** → `backend-src`
4. **Settings** → **Builder** → Selecione `Dockerfile` (não deixe em Auto-detect)
5. Railway vai usar o `Dockerfile` em `backend-src/Dockerfile`

**Variáveis de Ambiente:**
```bash
ConnectionStrings__DefaultConnection=Host=aws-0-us-west-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.alxtzjmtclopraayehfg;Password=MHd64o*cLZJ@Bv8
Supabase__Url=https://alxtzjmtclopraayehfg.supabase.co
Supabase__ServiceRoleKey=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFseHR6am10Y2xvcHJhYXllaGZnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njg1MTg4ODIsImV4cCI6MjA4NDA5NDg4Mn0.oFWXxd4lYM78kbBkW0khQQ-SCSNUZKlxaza2CLxm3Qk
Supabase__AnonKey=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFseHR6am10Y2xvcHJhYXllaGZnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njg1MTg4ODIsImV4cCI6MjA4NDA5NDg4Mn0.oFWXxd4lYM78kbBkW0khQQ-SCSNUZKlxaza2CLxm3Qk
Jwt__Secret=AstraFuture2026-Super-Secret-Key-Min-32-Chars-JWT-Signing
Jwt__Issuer=AstraFuture
Jwt__Audience=AstraFuture
Jwt__ExpirationMinutes=1440
ASPNETCORE_ENVIRONMENT=Production
```

#### Serviço 2: Frontend
1. No Railway, crie "New Service" → "Deploy from GitHub repo"
2. Escolha o repositório `astrafuture`
3. **Settings** → **Root Directory** → `frontend`
4. Railway vai detectar o `Dockerfile` em `frontend/Dockerfile`

**Variáveis de Ambiente:**
```bash
NEXT_PUBLIC_API_URL=https://[URL-DO-BACKEND-RAILWAY]/api
NEXT_PUBLIC_SUPABASE_URL=https://alxtzjmtclopraayehfg.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFseHR6am10Y2xvcHJhYXllaGZnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3Njg1MTg4ODIsImV4cCI6MjA4NDA5NDg4Mn0.oFWXxd4lYM78kbBkW0khQQ-SCSNUZKlxaza2CLxm3Qk
```

---

## 🐳 Dockerfiles

### Backend: `backend-src/Dockerfile`
```dockerfile
# Dockerfile para AstraFuture Backend (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todo o projeto de uma vez
COPY . .

# Restaurar e publicar
RUN dotnet restore "AstraFuture.Api/AstraFuture.Api.csproj"
RUN dotnet publish "AstraFuture.Api/AstraFuture.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "AstraFuture.Api.dll"]
```

### Frontend: `frontend/Dockerfile`
```dockerfile
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:20-alpine AS runner
WORKDIR /app
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
COPY --from=builder /app/public ./public

ENV NODE_ENV=production
ENV PORT=3000

EXPOSE 3000

CMD ["node", "server.js"]
```

---

## ✅ Checklist

- [ ] Criar serviço Backend no Railway com root directory `backend-src`
- [ ] Adicionar variáveis de ambiente do backend
- [ ] Aguardar deploy do backend e copiar a URL gerada
- [ ] Criar serviço Frontend no Railway com root directory `frontend`
- [ ] Adicionar variáveis de ambiente do frontend (incluindo URL do backend)
- [ ] Aguardar deploy do frontend
- [ ] Testar a aplicação completa

---

## 🆘 Troubleshooting

**Erro: "Railpack could not determine how to build"**
→ Configure o Root Directory nas Settings do serviço

**Erro: "Dockerfile does not exist" ou Railpack errors**
→ Nas Settings do serviço, vá em **Builder** e selecione `Dockerfile` manualmente
→ O Dockerfile está em `backend-src/Dockerfile` (root do backend-src)

**Erro: Build falhou no Next.js**
→ Verifique se o Dockerfile está em `frontend/Dockerfile`

**Erro: 404 nas chamadas da API**
→ Verifique se a URL do backend está correta no frontend (NEXT_PUBLIC_API_URL)

**Erro: CORS**
→ Backend precisa aceitar requisições do domínio do frontend Railway
