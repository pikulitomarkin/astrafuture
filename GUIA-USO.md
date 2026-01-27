# 🚀 AstraFuture - Guia Rápido de Uso

## 📋 Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [Conta Supabase](https://supabase.com) (grátis)

## 🔧 Configuração Inicial

### 1. Configurar Supabase

1. Crie uma conta em https://supabase.com
2. Crie um novo projeto
3. Vá em **Settings > API** e copie:
   - `Project URL`
   - `anon public key`
   - `service_role key`
4. Vá em **SQL Editor** e execute o schema: `database/schema.sql`

### 2. Configurar Backend

Crie o arquivo `backend-src/AstraFuture.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Supabase": {
    "Url": "https://SEU-PROJETO.supabase.co",
    "ServiceRoleKey": "sua-service-role-key",
    "AnonKey": "sua-anon-key"
  },
  "Jwt": {
    "Secret": "sua-chave-secreta-minimo-32-caracteres-aqui",
    "Issuer": "AstraFuture",
    "Audience": "AstraFuture",
    "ExpirationMinutes": 1440
  }
}
```

**Iniciar Backend:**
```bash
cd backend-src/AstraFuture.Api
dotnet run
```

O backend estará rodando em: http://localhost:5000

### 3. Configurar Frontend

Crie o arquivo `frontend/.env.local`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
NEXT_PUBLIC_SUPABASE_URL=https://SEU-PROJETO.supabase.co
NEXT_PUBLIC_SUPABASE_ANON_KEY=sua-anon-key
```

**Instalar dependências e iniciar:**
```bash
cd frontend
npm install
npm run dev
```

O frontend estará rodando em: http://localhost:3000

## 🎯 Usando o Sistema

### Primeiro Acesso

1. Acesse http://localhost:3000
2. Clique em **"Criar conta"**
3. Preencha:
   - Nome da empresa
   - E-mail
   - Senha (mínimo 6 caracteres)
4. Clique em **"Criar Conta"**

Você será automaticamente logado e redirecionado para o dashboard!

### Fluxo Básico

#### 1. Cadastrar Clientes
1. No menu lateral, clique em **"Clientes"**
2. Clique no botão **"Novo Cliente"**
3. Preencha:
   - Nome (obrigatório)
   - Telefone (obrigatório)
   - E-mail (opcional)
4. Clique em **"Criar"**

#### 2. Criar Agendamentos
1. No menu lateral, clique em **"Agendamentos"**
2. Clique no botão **"Novo Agendamento"**
3. Preencha:
   - Cliente (selecione da lista)
   - Data/Hora de início
   - Data/Hora de fim
   - Status (Agendado, Confirmado, etc.)
   - Observações (opcional)
4. Clique em **"Criar"**

#### 3. Gerenciar Agendamentos
- **Editar:** Clique no ícone de lápis no card do agendamento
- **Excluir:** Clique no ícone de lixeira
- **Visualizar:** Todos os agendamentos aparecem em cards com:
  - Data e horário
  - Nome do cliente
  - Status com cor
  - Observações

## 📊 Dashboard

O dashboard mostra:
- **Agendamentos Hoje:** Quantos agendamentos você tem hoje
- **Total de Clientes:** Quantos clientes cadastrados
- **Próximos 7 dias:** Agendamentos na próxima semana
- **Próximos Agendamentos:** Lista dos 3 próximos agendamentos

## 🎨 Status dos Agendamentos

- 🔵 **Agendado** (scheduled) - Agendamento criado
- 🟢 **Confirmado** (confirmed) - Cliente confirmou presença
- ⚪ **Concluído** (completed) - Atendimento realizado
- 🔴 **Cancelado** (cancelled) - Agendamento cancelado
- 🟠 **Não compareceu** (no_show) - Cliente faltou

## 🔐 Segurança

- Cada empresa tem seus dados isolados (multi-tenancy)
- Autenticação com JWT
- Senhas criptografadas
- API protegida (requer token)

## 🛠️ Troubleshooting

### Backend não inicia
- Verifique se o .NET 10 SDK está instalado: `dotnet --version`
- Verifique se as configurações do Supabase estão corretas
- Verifique se o schema foi executado no Supabase

### Frontend não conecta
- Verifique se o backend está rodando (http://localhost:5000)
- Verifique o arquivo `.env.local`
- Abra o console do navegador (F12) para ver erros

### Erro de autenticação
- Limpe o localStorage do navegador (F12 > Application > Local Storage > Clear)
- Faça logout e login novamente

## 📞 Suporte

Para dúvidas ou problemas, verifique os arquivos de documentação:
- `SETUP-SUPABASE.md` - Configuração detalhada do Supabase
- `SETUP-FRONTEND.md` - Configuração do frontend
- `api/README.md` - Documentação da API
- `backend/README.md` - Arquitetura do backend

## 🚀 Próximas Funcionalidades

Planejadas para as próximas versões:
- Visualização em calendário
- Notificações por e-mail/SMS
- WhatsApp bot para agendamentos
- Relatórios e analytics
- Gestão de recursos (profissionais, salas)
- Exportação de dados

---

**AstraFuture** - Sistema de Agendamentos para Pequenos e Médios Negócios
