# 🚀 Deploy no Railway - Guia Completo

**Backend:** Railway  
**Frontend:** Vercel  
**Tempo estimado:** 30-45 minutos

---

## 🎯 Por Que Railway?

### Vantagens
- ✅ **Setup simples** - Deploy em minutos
- ✅ **Free tier generoso** - $5 crédito/mês
- ✅ **Git integration** - Deploy automático a cada push
- ✅ **Suporte .NET 9** - Funciona perfeitamente
- ✅ **Variáveis de ambiente** - Interface amigável
- ✅ **Logs em tempo real** - Debug fácil
- ✅ **SSL automático** - HTTPS gratuito
- ✅ **Banco de dados** - PostgreSQL incluído (se quiser)

### Comparado com Fly.io
| Feature | Railway | Fly.io |
|---------|---------|--------|
| Interface | 🟢 Mais amigável | 🟡 Menos intuitiva |
| CLI | 🟢 Opcional | 🔴 Obrigatório |
| Free tier | 🟢 $5/mês | 🟢 Bom |
| Deploy | 🟢 Git push | 🟡 flyctl deploy |
| .NET support | 🟢 Nativo | 🟢 Bom |

**Escolha:** Railway é mais simples para começar!

---

## 📋 Pré-requisitos

- [ ] Conta no Railway (criar em railway.app)
- [ ] Conta no Vercel (criar em vercel.com)
- [ ] Conta no GitHub
- [ ] Projeto no GitHub (fazer push do código)
- [ ] Supabase configurado e rodando

---

## 🔧 Parte 1: Deploy do Backend (Railway)

### Passo 1: Criar Conta no Railway

1. Acesse https://railway.app
2. Clique em "Start a New Project"
3. Faça login com GitHub
4. Autorize o Railway a acessar seus repos

### Passo 2: Criar Novo Projeto

1. No dashboard, clique em "New Project"
2. Selecione "Deploy from GitHub repo"
3. Escolha o repositório `Astrafuture`
4. Selecione a branch `main`

### Passo 3: Configurar o Projeto

Railway vai detectar automaticamente que é um projeto .NET!

**Configurações importantes:**

1. **Root Directory:**
   ```
   backend-src/AstraFuture.Api
   ```

2. **Build Command:** (Railway detecta automaticamente)
   ```bash
   dotnet publish -c Release -o out
   ```

3. **Start Command:**
   ```bash
   dotnet out/AstraFuture.Api.dll
   ```

4. **Port:** Railway configura automaticamente
   - Geralmente usa `$PORT` variável de ambiente

### Passo 4: Configurar Variáveis de Ambiente

No dashboard do Railway, vá em **Variables** e adicione:

```bash
# Supabase
SUPABASE_URL=https://seu-projeto.supabase.co
SUPABASE_KEY=sua_service_role_key
SUPABASE_JWT_SECRET=seu_jwt_secret

# ASP.NET
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT

# CORS (será o domínio Vercel)
ALLOWED_ORIGINS=https://seu-app.vercel.app

# JWT
JWT_SECRET=seu_jwt_secret_super_seguro_aqui
JWT_ISSUER=AstraFuture
JWT_AUDIENCE=AstraFuture.Users
JWT_EXPIRY_MINUTES=1440
```

**Importante:** Copie esses valores do seu `appsettings.Development.json`

### Passo 5: Ajustar appsettings.json

Certifique-se que seu `appsettings.json` lê variáveis de ambiente:

```json
{
  "Supabase": {
    "Url": "${SUPABASE_URL}",
    "Key": "${SUPABASE_KEY}",
    "JwtSecret": "${SUPABASE_JWT_SECRET}"
  },
  "Jwt": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpiryMinutes": "${JWT_EXPIRY_MINUTES}"
  },
  "AllowedOrigins": "${ALLOWED_ORIGINS}"
}
```

Ou use código C# para ler de `Environment.GetEnvironmentVariable()`.

### Passo 6: Deploy!

1. Faça commit das mudanças:
   ```bash
   git add .
   git commit -m "chore: configurar para Railway"
   git push origin main
   ```

2. Railway vai detectar e fazer deploy automaticamente!

3. Acompanhe os logs em tempo real no dashboard

### Passo 7: Testar a API

1. Railway vai gerar uma URL tipo: `https://seu-app.up.railway.app`

2. Teste os endpoints:
   ```bash
   # Health check
   curl https://seu-app.up.railway.app/health
   
   # Swagger (se habilitado em produção)
   https://seu-app.up.railway.app/swagger
   ```

3. Copie a URL - você vai precisar no frontend!

---

## 🎨 Parte 2: Deploy do Frontend (Vercel)

### Passo 1: Preparar o Frontend

1. Atualize `.env.production` no frontend:
   ```bash
   NEXT_PUBLIC_API_URL=https://seu-app.up.railway.app/api
   ```

2. Commit:
   ```bash
   git add .
   git commit -m "chore: configurar API URL para produção"
   git push origin main
   ```

### Passo 2: Criar Projeto no Vercel

1. Acesse https://vercel.com
2. Faça login com GitHub
3. Clique em "Add New Project"
4. Importe o repositório `Astrafuture`

### Passo 3: Configurar Build

**Root Directory:**
```
frontend
```

**Framework Preset:**
- Selecione "Next.js"

**Build Command:** (Vercel detecta automaticamente)
```bash
npm run build
```

**Output Directory:**
```
.next
```

### Passo 4: Variáveis de Ambiente

Na aba **Environment Variables**, adicione:

```bash
NEXT_PUBLIC_API_URL=https://seu-app.up.railway.app/api
```

### Passo 5: Deploy!

1. Clique em "Deploy"
2. Vercel vai buildar e fazer deploy
3. Aguarde ~2 minutos

### Passo 6: Testar o App

1. Vercel vai gerar uma URL: `https://seu-app.vercel.app`
2. Acesse e teste:
   - Criar conta
   - Fazer login
   - Ver dashboard
   - Ver appointments
3. Teste em mobile também!

---

## 🔄 Parte 3: Conectar Backend e Frontend

### Passo 1: Atualizar CORS no Backend

No Railway, atualize a variável `ALLOWED_ORIGINS`:

```bash
ALLOWED_ORIGINS=https://seu-app.vercel.app,https://seu-app-*.vercel.app
```

O `*` permite preview deployments do Vercel.

### Passo 2: Re-deploy

Railway vai detectar a mudança e re-deployar automaticamente.

### Passo 3: Testar Integração

1. Acesse `https://seu-app.vercel.app`
2. Crie uma conta
3. Faça login
4. Veja se tudo funciona

Se der erro de CORS, verifique:
- URL da API está correta no frontend?
- CORS está configurado no backend?
- Railway re-deployou?

---

## 🎯 Troubleshooting

### Backend não inicia

**Problema:** Build falha ou app não inicia

**Soluções:**
1. Verifique os logs no Railway
2. Certifique-se que `ASPNETCORE_URLS` está configurado
3. Verifique que todas as env vars estão corretas
4. Tente rodar localmente com as mesmas env vars

### Frontend não conecta com Backend

**Problema:** Erro de CORS ou 404

**Soluções:**
1. Verifique se `NEXT_PUBLIC_API_URL` está correto
2. Teste a API diretamente: `curl https://seu-app.up.railway.app/health`
3. Verifique CORS no backend
4. Limpe o cache do navegador

### Database connection failed

**Problema:** Backend não conecta com Supabase

**Soluções:**
1. Verifique `SUPABASE_URL` e `SUPABASE_KEY`
2. Teste a conexão do Supabase SQL Editor
3. Verifique se o IP do Railway está permitido no Supabase

### Build muito lento

**Problema:** Deploy demora muito

**Soluções:**
1. Railway free tier é mais lento
2. Considere upgrade para Pro
3. Otimize o build (remova dependências não usadas)

---

## 📊 Monitoramento

### Railway

**Ver logs:**
```bash
railway logs
```

Ou no dashboard: Project → Deployments → View Logs

**Métricas:**
- CPU usage
- Memory usage
- Network
- Deployments

### Vercel

**Ver logs:**
No dashboard: Project → Deployments → View Function Logs

**Analytics:**
- Pageviews
- Performance (Core Web Vitals)
- Errors

---

## 💰 Custos

### Railway Free Tier
- $5 crédito/mês
- Suficiente para testes e MVPs
- ~500 horas/mês de uptime

**Quando fazer upgrade:**
- Mais de 100 usuários ativos
- Precisa de performance melhor
- Quer custom domain

### Vercel Free Tier
- 100 GB bandwidth/mês
- Unlimited deployments
- Suficiente para começar

**Quando fazer upgrade:**
- Mais de 1000 usuários
- Precisa de analytics avançados
- Quer password protection

---

## 🚀 Próximos Passos Após Deploy

### Semana 2
- [ ] Configurar domínio customizado
- [ ] Configurar monitoring (Sentry)
- [ ] Setup CI/CD com testes
- [ ] Configurar backups do DB

### Semana 3
- [ ] Performance optimization
- [ ] SEO básico
- [ ] Error tracking
- [ ] User analytics

---

## 📝 Checklist de Deploy

Use este checklist:

### Backend (Railway)
- [ ] Projeto criado no Railway
- [ ] Repo conectado
- [ ] Env vars configuradas
- [ ] Build successful
- [ ] App está rodando
- [ ] Health endpoint responde
- [ ] Swagger funciona (se habilitado)
- [ ] Logs sem erros

### Frontend (Vercel)
- [ ] Projeto criado no Vercel
- [ ] Repo conectado
- [ ] Env vars configuradas
- [ ] Build successful
- [ ] App está acessível
- [ ] Páginas carregam
- [ ] API conecta
- [ ] Sem erros no console

### Integração
- [ ] CORS configurado
- [ ] Frontend chama API
- [ ] Login funciona
- [ ] CRUD funciona
- [ ] Testado em mobile
- [ ] SSL/HTTPS funciona

---

## 🎉 Sucesso!

Se todos os checkboxes acima estão marcados, **PARABÉNS!** 🎉

Seu app está online e funcionando!

**URLs para compartilhar:**
- Frontend: `https://seu-app.vercel.app`
- API (Swagger): `https://seu-app.up.railway.app/swagger`

---

**Última atualização:** 16 Janeiro 2026  
**Próxima revisão:** Após primeiro deploy
