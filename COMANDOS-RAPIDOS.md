# ⚡ Comandos Rápidos - AstraFuture

Comandos úteis para desenvolvimento diário.

---

## 🚀 Iniciar Projeto

### Backend
```powershell
cd d:\Astrafuture\backend-src\AstraFuture.Api
dotnet run
```
**URL:** http://localhost:5000  
**Swagger:** http://localhost:5000/swagger

### Frontend
```powershell
cd d:\Astrafuture\frontend
npm run dev
```
**URL:** http://localhost:3000

---

## 🔧 Build e Teste

### Backend
```powershell
# Restaurar dependências
cd d:\Astrafuture\backend-src
dotnet restore

# Build
dotnet build

# Build com otimizações
dotnet build -c Release

# Rodar testes
cd AstraFuture.Tests
dotnet test

# Watch mode (hot reload)
cd AstraFuture.Api
dotnet watch run
```

### Frontend
```powershell
cd d:\Astrafuture\frontend

# Instalar dependências
npm install

# Dev mode
npm run dev

# Build para produção
npm run build

# Rodar build de produção
npm start

# Lint
npm run lint

# Lint e fix
npm run lint -- --fix
```

---

## 🧹 Limpeza

### Backend
```powershell
cd d:\Astrafuture\backend-src

# Limpar build artifacts
dotnet clean

# Limpar tudo (incluindo obj)
Get-ChildItem -Include bin,obj -Recurse | Remove-Item -Force -Recurse
```

### Frontend
```powershell
cd d:\Astrafuture\frontend

# Limpar node_modules e cache
Remove-Item -Recurse -Force node_modules, .next, package-lock.json

# Reinstalar
npm install
```

---

## 📦 Gerenciamento de Dependências

### Backend
```powershell
cd d:\Astrafuture\backend-src\AstraFuture.Api

# Adicionar pacote
dotnet add package NomeDoPacote

# Remover pacote
dotnet remove package NomeDoPacote

# Listar pacotes
dotnet list package

# Atualizar pacotes
dotnet add package NomeDoPacote --version x.x.x
```

### Frontend
```powershell
cd d:\Astrafuture\frontend

# Adicionar pacote
npm install nome-do-pacote

# Adicionar pacote dev
npm install -D nome-do-pacote

# Remover pacote
npm uninstall nome-do-pacote

# Atualizar pacote
npm update nome-do-pacote

# Listar pacotes desatualizados
npm outdated
```

---

## 🗄️ Database

### Supabase SQL Editor
```sql
-- Ver todas as tabelas
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';

-- Ver appointments
SELECT * FROM appointments LIMIT 10;

-- Ver customers
SELECT * FROM customers LIMIT 10;

-- Contar registros
SELECT 
    (SELECT COUNT(*) FROM appointments) as appointments,
    (SELECT COUNT(*) FROM customers) as customers,
    (SELECT COUNT(*) FROM tenants) as tenants;

-- Limpar dados de teste
DELETE FROM appointments;
DELETE FROM customers;
-- Cuidado: não delete tenants se tiver dados importantes!
```

### Migrations
```powershell
cd d:\Astrafuture\database

# Aplicar migration
# (no Supabase SQL Editor, copie e cole o conteúdo do arquivo)
```

---

## 🐛 Debug

### Backend
```powershell
cd d:\Astrafuture\backend-src\AstraFuture.Api

# Rodar com logs detalhados
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Ver variáveis de ambiente
Get-ChildItem Env:

# Limpar cache do .NET
dotnet nuget locals all --clear
```

### Frontend
```powershell
cd d:\Astrafuture\frontend

# Limpar cache Next.js
Remove-Item -Recurse -Force .next

# Verificar erros TypeScript
npx tsc --noEmit

# Verificar erros ESLint
npm run lint

# Node com debug
$env:NODE_OPTIONS="--inspect"
npm run dev
```

---

## 🔍 Verificação Rápida

### Status Geral
```powershell
# Verificar versões instaladas
node --version
npm --version
dotnet --version

# Verificar se portas estão em uso
# Backend (5000)
netstat -ano | findstr :5000

# Frontend (3000)
netstat -ano | findstr :3000
```

### Health Check
```powershell
# Backend health
curl http://localhost:5000/health

# Frontend (abrir no navegador)
start http://localhost:3000
```

---

## 📝 Git

### Workflow Comum
```powershell
# Status
git status

# Ver mudanças
git diff

# Adicionar arquivos
git add .

# Commit
git commit -m "feat: descrição da feature"

# Push
git push origin main

# Pull
git pull origin main

# Ver histórico
git log --oneline -10

# Criar branch
git checkout -b feature/nova-feature

# Trocar branch
git checkout main
```

### Reset e Limpeza
```powershell
# Descartar mudanças locais
git checkout -- .

# Descartar mudanças em arquivo específico
git checkout -- arquivo.txt

# Remover arquivos não rastreados
git clean -fd

# Reset para último commit
git reset --hard HEAD
```

---

## 🚀 Deploy (Futuro)

### Backend (Railway)
```powershell
cd d:\Astrafuture\backend-src

# Instalar Railway CLI
npm i -g @railway/cli

# Login
railway login

# Link ao projeto
railway link

# Deploy
railway up

# Ver logs
railway logs

# Abrir dashboard
railway open
```

### Frontend (Vercel)
```powershell
cd d:\Astrafuture\frontend

# Login
npx vercel login

# Deploy
npx vercel

# Deploy produção
npx vercel --prod

# Ver logs
npx vercel logs
```

---

## 🧪 Testes Manuais

### Fluxo Completo
```powershell
# 1. Iniciar backend
cd d:\Astrafuture\backend-src\AstraFuture.Api
dotnet run

# 2. Em outro terminal, iniciar frontend
cd d:\Astrafuture\frontend
npm run dev

# 3. Abrir navegador
start http://localhost:3000

# 4. Testar:
# - Criar conta
# - Fazer login
# - Ver dashboard
# - Ver appointments
# - Excluir appointment
# - Fazer logout
```

---

## 📊 Análise

### Backend
```powershell
cd d:\Astrafuture\backend-src

# Linhas de código
Get-ChildItem -Recurse -Include *.cs | Get-Content | Measure-Object -Line

# Arquivos
Get-ChildItem -Recurse -Include *.cs | Measure-Object

# Projetos
Get-ChildItem -Recurse -Include *.csproj | Select-Object Name
```

### Frontend
```powershell
cd d:\Astrafuture\frontend

# Linhas de código
Get-ChildItem -Recurse -Include *.ts,*.tsx | Get-Content | Measure-Object -Line

# Componentes
Get-ChildItem -Recurse -Path src/components -Include *.tsx

# Páginas
Get-ChildItem -Recurse -Path src/app -Include page.tsx
```

---

## 🔐 Environment Variables

### Backend
```powershell
# Ver appsettings
cat backend-src\AstraFuture.Api\appsettings.json

# Editar appsettings
notepad backend-src\AstraFuture.Api\appsettings.Development.json
```

### Frontend
```powershell
# Ver .env.local (se existir)
cat frontend\.env.local

# Editar .env.local
notepad frontend\.env.local

# Variáveis importantes:
# NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

---

## 💡 Dicas

### Produtividade
```powershell
# Abrir VS Code no backend
code backend-src

# Abrir VS Code no frontend
code frontend

# Abrir múltiplos terminais (Windows Terminal)
# Ctrl+Shift+T - Novo tab
# Ctrl+Shift+D - Split pane
```

### Aliases (Opcional)
Adicione ao seu PowerShell Profile:

```powershell
# Ver profile
$PROFILE

# Editar profile
notepad $PROFILE

# Adicionar aliases:
function afb { cd d:\Astrafuture\backend-src\AstraFuture.Api; dotnet run }
function aff { cd d:\Astrafuture\frontend; npm run dev }
function afroot { cd d:\Astrafuture }
```

Depois:
```powershell
# Reiniciar PowerShell e usar:
afb  # Iniciar backend
aff  # Iniciar frontend
afroot  # Ir para raiz
```

---

## 🆘 Troubleshooting

### "Port already in use"
```powershell
# Encontrar processo
netstat -ano | findstr :5000

# Matar processo (substitua PID)
taskkill /PID numero_do_pid /F
```

### "Module not found"
```powershell
# Frontend
cd frontend
Remove-Item -Recurse -Force node_modules
npm install

# Backend
cd backend-src
dotnet restore
```

### "Cannot connect to database"
```
1. Verificar Supabase está online
2. Verificar connection string em appsettings.json
3. Testar conexão no Supabase SQL Editor
```

---

**💪 Você tem tudo que precisa! Bora codar!**
