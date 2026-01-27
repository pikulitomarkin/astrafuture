# 🚀 Checklist de Deploy - AstraFuture

## ✅ Pré-Deploy (Local)

### Supabase
- [ ] Criar projeto no Supabase
- [ ] Copiar URL e keys (Settings > API)
- [ ] Executar schema.sql no SQL Editor
- [ ] Verificar tabelas criadas (Table Editor)
- [ ] Testar conexão local

### Backend
- [ ] Criar `appsettings.Development.json` com credenciais
- [ ] Compilar sem erros: `dotnet build`
- [ ] Executar: `dotnet run`
- [ ] Testar endpoint: http://localhost:5000/swagger
- [ ] Testar POST /api/auth/register
- [ ] Testar POST /api/auth/login

### Frontend
- [ ] Criar `.env.local` com credenciais
- [ ] Instalar dependências: `npm install`
- [ ] Executar: `npm run dev`
- [ ] Testar http://localhost:3000
- [ ] Criar conta de teste
- [ ] Criar cliente de teste
- [ ] Criar agendamento de teste

---

## 🌐 Deploy em Produção

### 1. Deploy Backend (Railway)

**Criar Projeto:**
```bash
# Instalar Railway CLI
npm install -g @railway/cli

# Login
railway login

# Criar projeto
railway init
```

**Configurar Variáveis:**
No Railway Dashboard > Variables:
```
SUPABASE_URL=https://seu-projeto.supabase.co
SUPABASE_SERVICE_ROLE_KEY=sua-service-key
SUPABASE_ANON_KEY=sua-anon-key
JWT_SECRET=sua-chave-secreta-32-caracteres
JWT_ISSUER=AstraFuture
JWT_AUDIENCE=AstraFuture
JWT_EXPIRATION_MINUTES=1440
ASPNETCORE_ENVIRONMENT=Production
```

**Deploy:**
```bash
cd backend-src/AstraFuture.Api
railway up
```

**Testar:**
- Copiar URL gerada (ex: https://astrafuture.railway.app)
- Testar: https://astrafuture.railway.app/swagger

### 2. Deploy Frontend (Vercel)

**Criar Projeto:**
```bash
# Instalar Vercel CLI
npm install -g vercel

# Deploy
cd frontend
vercel
```

**Configurar Variáveis:**
No Vercel Dashboard > Settings > Environment Variables:
```
NEXT_PUBLIC_API_URL=https://astrafuture.railway.app/api
NEXT_PUBLIC_SUPABASE_URL=https://seu-projeto.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=sua-anon-key
```

**Deploy Production:**
```bash
vercel --prod
```

**Testar:**
- Acessar URL gerada
- Criar conta
- Testar todas funcionalidades

### 3. Configurar Domínio (Opcional)

**Backend (Railway):**
1. Settings > Domains
2. Add Custom Domain: `api.seudominio.com`
3. Adicionar registro CNAME no DNS

**Frontend (Vercel):**
1. Settings > Domains
2. Add: `seudominio.com` e `www.seudominio.com`
3. Adicionar registros A/CNAME no DNS

---

## 🔒 Segurança em Produção

### Backend
- [ ] Gerar JWT_SECRET forte (32+ caracteres aleatórios)
- [ ] Configurar CORS apenas para domínio do frontend
- [ ] Desabilitar Swagger em produção
- [ ] Configurar rate limiting
- [ ] Habilitar HTTPS only

### Frontend
- [ ] Nunca commitar arquivos `.env*`
- [ ] Usar apenas `NEXT_PUBLIC_*` para variáveis públicas
- [ ] Verificar bundle size: `npm run build`

### Supabase
- [ ] Configurar RLS policies
- [ ] Limitar IPs permitidos (opcional)
- [ ] Configurar backup automático
- [ ] Monitorar uso do banco

---

## 📊 Monitoramento

### Railway (Backend)
- [ ] Configurar alertas de erro
- [ ] Monitorar CPU/RAM
- [ ] Verificar logs: `railway logs`

### Vercel (Frontend)
- [ ] Configurar Vercel Analytics
- [ ] Monitorar Web Vitals
- [ ] Verificar logs de build

### Supabase
- [ ] Monitorar Database Health
- [ ] Verificar API requests
- [ ] Configurar alertas de quota

---

## ✅ Checklist Pós-Deploy

### Funcional
- [ ] Criar conta funciona
- [ ] Login funciona
- [ ] Dashboard carrega
- [ ] Criar cliente funciona
- [ ] Criar agendamento funciona
- [ ] Editar funciona
- [ ] Deletar funciona
- [ ] Logout funciona
- [ ] Busca funciona
- [ ] Dados persistem após refresh

### Performance
- [ ] Tempo de carregamento < 3s
- [ ] API responde < 500ms
- [ ] Sem erros no console
- [ ] Funciona em mobile
- [ ] Funciona em diferentes navegadores

### Segurança
- [ ] HTTPS habilitado
- [ ] JWT expira corretamente
- [ ] Multi-tenancy isolado
- [ ] Sem credenciais expostas
- [ ] CORS configurado

---

## 🎯 Alternativas de Deploy

### Backend

**Opção 1: Railway** (Recomendado)
- ✅ Fácil
- ✅ Gratuito (500h/mês)
- ✅ Auto-deploy
- ❌ Limite de recursos

**Opção 2: Azure**
- ✅ Escalável
- ✅ Integração Microsoft
- ❌ Mais caro
- ❌ Configuração complexa

**Opção 3: AWS (Elastic Beanstalk)**
- ✅ Muito escalável
- ✅ Completo
- ❌ Caro
- ❌ Curva de aprendizado

**Opção 4: DigitalOcean**
- ✅ Barato
- ✅ Controle total
- ❌ Requer mais configuração

### Frontend

**Opção 1: Vercel** (Recomendado)
- ✅ Otimizado para Next.js
- ✅ Gratuito
- ✅ Deploy automático
- ✅ Edge functions

**Opção 2: Netlify**
- ✅ Gratuito
- ✅ Fácil
- ❌ Menos otimizado para Next.js

**Opção 3: AWS Amplify**
- ✅ Integração AWS
- ❌ Mais caro

---

## 💰 Custos Estimados

### Plano Gratuito (MVP)
```
Supabase:  R$ 0/mês (até 500MB)
Railway:   R$ 0/mês (500h)
Vercel:    R$ 0/mês (100GB bandwidth)
---
TOTAL:     R$ 0/mês
```

### Plano Pago (Escalado)
```
Supabase Pro:    R$ 125/mês (8GB database)
Railway Hobby:   R$ 50/mês (1GB RAM)
Vercel Pro:      R$ 100/mês (1TB bandwidth)
Domínio:         R$ 40/ano
---
TOTAL:           ~R$ 280/mês
```

---

## 📞 Troubleshooting

### Backend não conecta ao Supabase
- Verificar SUPABASE_URL correto
- Verificar SERVICE_ROLE_KEY correto
- Verificar firewall/network

### Frontend não conecta ao Backend
- Verificar NEXT_PUBLIC_API_URL correto
- Verificar CORS habilitado no backend
- Abrir console (F12) e ver erro

### Deploy falha
- Verificar logs: `railway logs` ou `vercel logs`
- Verificar variáveis de ambiente
- Verificar build passa: `npm run build`

---

## 🎉 Sucesso!

Após completar este checklist, seu sistema estará:
- ✅ Online 24/7
- ✅ Acessível globalmente
- ✅ Escalável
- ✅ Seguro
- ✅ Monitorado

**Parabéns! 🚀**

---

## 📚 Documentação Adicional

- [Railway Docs](https://docs.railway.app/)
- [Vercel Docs](https://vercel.com/docs)
- [Supabase Docs](https://supabase.com/docs)
- [Next.js Deploy](https://nextjs.org/docs/deployment)

---

**Última atualização:** 27 de Janeiro de 2026
