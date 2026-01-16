# AstraFuture Frontend

Sistema de gerenciamento de agendamentos multi-tenant - Interface web.

## 🚀 Tecnologias

- **Next.js 15** - Framework React
- **TypeScript** - Tipagem estática
- **Tailwind CSS** - Estilização
- **React Query** - Gerenciamento de estado servidor
- **Zustand** - Gerenciamento de estado cliente
- **Axios** - Cliente HTTP
- **Lucide React** - Ícones
- **Sonner** - Notificações toast

## 📦 Instalação

```bash
# Instalar dependências
npm install

# Copiar arquivo de ambiente
cp .env.local.example .env.local

# Configurar variáveis de ambiente
# Edite .env.local e configure:
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

## 🏃‍♂️ Como Executar

```bash
# Modo desenvolvimento
npm run dev

# Build para produção
npm run build

# Iniciar produção
npm start
```

O aplicativo estará disponível em `http://localhost:3000`

## 📁 Estrutura do Projeto

```
frontend/
├── src/
│   ├── app/                    # Rotas Next.js (App Router)
│   │   ├── login/             # Página de login
│   │   ├── register/          # Página de registro
│   │   └── dashboard/         # Dashboard protegido
│   │       ├── appointments/  # Página de agendamentos
│   │       └── layout.tsx     # Layout do dashboard
│   ├── components/            # Componentes React
│   │   ├── ui/               # Componentes UI básicos
│   │   ├── dashboard/        # Componentes do dashboard
│   │   └── appointments/     # Componentes de agendamentos
│   ├── hooks/                # Custom hooks
│   │   ├── use-auth.ts      # Hook de autenticação
│   │   └── use-appointments.ts
│   ├── lib/                  # Utilitários
│   │   ├── api-client.ts    # Cliente da API
│   │   └── utils.ts         # Funções auxiliares
│   ├── store/               # Stores Zustand
│   │   └── auth-store.ts   # Store de autenticação
│   └── types/              # Definições TypeScript
│       └── index.ts
├── public/                  # Arquivos estáticos
├── package.json
├── tsconfig.json
├── tailwind.config.js
└── next.config.js
```

## 🔑 Funcionalidades Implementadas

### Autenticação
- ✅ Login de usuário
- ✅ Registro de novo tenant
- ✅ Proteção de rotas
- ✅ Persistência de sessão

### Dashboard
- ✅ Layout com sidebar e header
- ✅ Navegação entre páginas
- ✅ Logout

### Agendamentos
- ✅ Listagem de agendamentos
- ✅ Cards com informações detalhadas
- ✅ Estados de loading
- ✅ Tratamento de erros
- ✅ Exclusão de agendamentos

## 🎨 Componentes UI

Os componentes UI são construídos com Tailwind CSS e seguem padrões modernos:

- `Button` - Botão customizável com variantes
- `Card` - Container para conteúdo
- `Input` - Campo de entrada
- `Label` - Label para formulários

## 🔄 Gerenciamento de Estado

### React Query
- Cache automático de requisições
- Sincronização de dados
- Invalidação inteligente

### Zustand
- Estado de autenticação
- Persistência no localStorage

## 🌐 API Client

O cliente da API (`api-client.ts`) fornece:

- Interceptors para autenticação
- Tratamento automático de erros 401
- Métodos tipados para todas as rotas

## 📱 Responsividade

O aplicativo é totalmente responsivo e funciona em:
- Desktop (1024px+)
- Tablet (768px - 1023px)
- Mobile (320px - 767px)

## 🚧 Próximas Funcionalidades

- [ ] Formulário de criação de agendamento
- [ ] Edição de agendamentos
- [ ] Filtros e busca
- [ ] Visualização em calendário
- [ ] Gestão de clientes
- [ ] Notificações em tempo real

## 📝 Notas

- O frontend está configurado para conectar com o backend em `http://localhost:5000/api`
- Certifique-se de que o backend está rodando antes de iniciar o frontend
- As credenciais são armazenadas no localStorage

## 🐛 Debug

Se tiver problemas:

1. Verifique se o backend está rodando
2. Verifique o console do navegador para erros
3. Verifique a aba Network para requisições falhadas
4. Limpe o localStorage se tiver problemas de autenticação

```javascript
// No console do navegador
localStorage.clear()
```
