# 🚀 AstraFuture - Sistema de Agendamentos

Sistema completo de gerenciamento de agendamentos para pequenos e médios negócios. Moderno, seguro e fácil de usar.

[![Status](https://img.shields.io/badge/Status-Pronto%20para%20Venda-brightgreen)]()
[![Backend](https://img.shields.io/badge/Backend-.NET%2010-blue)]()
[![Frontend](https://img.shields.io/badge/Frontend-Next.js%2015-black)]()

## ✨ Funcionalidades

### ✅ Implementado
- 🔐 **Autenticação** - Sistema completo de login e registro
- 📅 **Agendamentos** - CRUD completo com status
- 👥 **Clientes** - Gestão de clientes com busca
- 📊 **Dashboard** - Métricas em tempo real
- 🎨 **Interface Moderna** - Design responsivo e intuitivo
- 🔒 **Multi-tenancy** - Dados isolados por empresa
- 🌐 **API RESTful** - Backend completo documentado

### 🚧 Em Planejamento
- 📆 Visualização em calendário
- 📧 Notificações por email/SMS
- 💬 WhatsApp Bot
- 📈 Relatórios e analytics
- 🎯 Command Palette (Ctrl+K)

## 🛠️ Tecnologias

### Backend
- **.NET 10** - Framework moderno e performático
- **Clean Architecture** - Separação de responsabilidades
- **CQRS** com MediatR - Padrão de comandos e queries
- **Supabase** - Banco de dados PostgreSQL
- **Dapper** - ORM leve e rápido
- **JWT** - Autenticação segura

### Frontend
- **Next.js 15** - React framework com SSR
- **React 18** - Interface reativa
- **TailwindCSS** - Design system
- **React Query** - Gerenciamento de estado server
- **Radix UI** - Componentes acessíveis
- **TypeScript** - Tipagem estática

## 🚀 Início Rápido

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/)
- [Conta Supabase](https://supabase.com) (grátis)

### 1. Configurar Supabase
```bash
# Criar projeto em https://supabase.com
# Executar database/schema.sql no SQL Editor
# Copiar credenciais (Settings > API)
```

### 2. Backend
```bash
# Copiar configurações
cp backend-src/AstraFuture.Api/appsettings.Development.json.example \
   backend-src/AstraFuture.Api/appsettings.Development.json

# Editar appsettings.Development.json com suas credenciais

# Executar
cd backend-src/AstraFuture.Api
dotnet run
```

Backend em: http://localhost:5000

### 3. Frontend
```bash
# Copiar configurações
cp frontend/.env.local.example frontend/.env.local

# Editar .env.local com suas credenciais

# Instalar e executar
cd frontend
npm install
npm run dev
```

Frontend em: http://localhost:3000

## 📖 Documentação

- **[GUIA-USO.md](GUIA-USO.md)** - Guia completo de uso
- **[STATUS-PROJETO.md](STATUS-PROJETO.md)** - Status atual e roadmap
- **[CHECKLIST-DEPLOY.md](CHECKLIST-DEPLOY.md)** - Deploy em produção
- **[SETUP-SUPABASE.md](SETUP-SUPABASE.md)** - Configuração do banco
- **[api/README.md](api/README.md)** - Documentação da API
- **[backend/README.md](backend/README.md)** - Arquitetura do backend

## 🎯 Como Usar

### 1. Criar Conta
- Acesse http://localhost:3000
- Clique em "Criar conta"
- Preencha nome da empresa, email e senha
- Login automático

### 2. Cadastrar Clientes
- Menu lateral > "Clientes"
- Botão "Novo Cliente"
- Preencha nome, telefone e email
- Criar

### 3. Criar Agendamentos
- Menu lateral > "Agendamentos"
- Botão "Novo Agendamento"
- Selecione cliente, data/hora e status
- Criar

## 🏗️ Arquitetura

```
astrafuture/
├── backend-src/              # Backend .NET
│   ├── AstraFuture.Api/      # Controllers e endpoints
│   ├── AstraFuture.Application/  # Use cases (CQRS)
│   ├── AstraFuture.Domain/   # Entidades e regras de negócio
│   ├── AstraFuture.Infrastructure/  # Supabase, repos
│   └── AstraFuture.Tests/    # Testes unitários
│
├── frontend/                 # Frontend Next.js
│   ├── src/
│   │   ├── app/             # Páginas (App Router)
│   │   ├── components/      # Componentes React
│   │   ├── hooks/           # Custom hooks
│   │   ├── lib/             # Utilitários
│   │   └── types/           # TypeScript types
│   └── public/              # Assets estáticos
│
├── database/                # SQL schemas
└── docs/                    # Documentação
```

## 📊 Status do Projeto

✅ **MVP Completo** - Pronto para uso real
- Backend: 100%
- Frontend: 100%
- CRUD: 100%
- Auth: 100%
- Multi-tenancy: 100%

Ver [STATUS-PROJETO.md](STATUS-PROJETO.md) para detalhes completos.

## 🤝 Contribuindo

Este é um projeto MVP. Contribuições são bem-vindas!

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto é privado. Todos os direitos reservados.

## 📞 Suporte

Para dúvidas ou problemas:
1. Verifique a [documentação](docs/)
2. Consulte o [GUIA-USO.md](GUIA-USO.md)
3. Veja o [STATUS-PROJETO.md](STATUS-PROJETO.md)

---

**AstraFuture** - Transformando a gestão de agendamentos  
Desenvolvido com ❤️ usando .NET e Next.js

**Última atualização:** 27 de Janeiro de 2026
