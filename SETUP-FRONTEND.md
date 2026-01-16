# 🚀 Setup Frontend - AstraFuture

Guia rápido para configurar e rodar o frontend do AstraFuture.

## ✅ Pré-requisitos

- Node.js 18+ instalado
- npm ou yarn
- Backend rodando (porta 5000)

## 📦 Instalação

### 1. Navegar até a pasta do frontend

```bash
cd d:\Astrafuture\frontend
```

### 2. Instalar dependências

```bash
npm install
```

Ou se preferir yarn:

```bash
yarn install
```

### 3. Configurar variáveis de ambiente

Copie o arquivo de exemplo:

```bash
copy .env.example .env.local
```

Edite `.env.local` se necessário (já está configurado para localhost):

```env
NEXT_PUBLIC_API_URL=http://localhost:5000/api
```

## 🏃‍♂️ Executar

### Modo Desenvolvimento

```bash
npm run dev
```

O aplicativo estará disponível em: **http://localhost:3000**

### Build de Produção

```bash
npm run build
npm start
```

## 🧪 Testar a Aplicação

### 1. Criar uma conta

1. Acesse http://localhost:3000
2. Clique em "Criar conta"
3. Preencha:
   - Nome do Negócio: "Minha Empresa"
   - Email: "teste@teste.com"
   - Senha: "123456"
4. Clique em "Criar conta"

### 2. Fazer Login

1. Email: "teste@teste.com"
2. Senha: "123456"
3. Clique em "Entrar"

### 3. Explorar o Dashboard

Após o login, você verá:
- Dashboard com métricas
- Menu lateral com navegação
- Página de Agendamentos

## 📱 Páginas Disponíveis

| Rota | Descrição | Autenticação |
|------|-----------|--------------|
| `/` | Redirect para `/login` | Não |
| `/login` | Página de login | Não |
| `/register` | Página de registro | Não |
| `/dashboard` | Dashboard principal | Sim |
| `/dashboard/appointments` | Lista de agendamentos | Sim |
| `/dashboard/customers` | Lista de clientes (WIP) | Sim |

## 🎨 Funcionalidades Implementadas

### ✅ Autenticação
- [x] Login
- [x] Registro
- [x] Logout
- [x] Proteção de rotas
- [x] Persistência de sessão

### ✅ Dashboard
- [x] Layout com sidebar
- [x] Header dinâmico
- [x] Navegação
- [x] Cards de métricas

### ✅ Agendamentos
- [x] Listar todos
- [x] Visualizar detalhes
- [x] Excluir
- [x] Estados de loading
- [x] Tratamento de erros

### 🚧 Em Desenvolvimento
- [ ] Criar agendamento
- [ ] Editar agendamento
- [ ] Filtros e busca
- [ ] Gestão de clientes
- [ ] Visualização em calendário

## 🐛 Troubleshooting

### Erro: "Cannot connect to backend"

**Causa:** Backend não está rodando ou está em porta diferente.

**Solução:**
1. Certifique-se que o backend está rodando:
   ```bash
   cd d:\Astrafuture\backend-src\AstraFuture.Api
   dotnet run
   ```
2. Verifique a porta no appsettings.json do backend
3. Atualize `.env.local` com a URL correta

### Erro: "Unauthorized"

**Causa:** Token expirado ou inválido.

**Solução:**
1. Faça logout
2. Limpe o localStorage:
   ```javascript
   // No console do navegador (F12)
   localStorage.clear()
   ```
3. Faça login novamente

### Erro de compilação TypeScript

**Causa:** Tipos não encontrados.

**Solução:**
```bash
npm install --save-dev @types/node @types/react @types/react-dom
```

### Erro: "Module not found"

**Causa:** Dependência não instalada.

**Solução:**
```bash
rm -rf node_modules package-lock.json
npm install
```

## 📂 Estrutura de Pastas

```
frontend/
├── src/
│   ├── app/                    # Rotas (Next.js App Router)
│   │   ├── dashboard/         # Área protegida
│   │   ├── login/
│   │   └── register/
│   ├── components/            # Componentes React
│   │   ├── ui/               # Componentes base
│   │   ├── dashboard/        # Componentes do dashboard
│   │   └── appointments/     # Componentes de agendamentos
│   ├── hooks/                # Custom hooks
│   ├── lib/                  # Utilitários e API client
│   ├── store/               # Zustand stores
│   └── types/              # TypeScript types
├── public/                  # Arquivos estáticos
└── package.json
```

## 🔑 Credenciais de Teste

Para testes, você pode criar uma conta ou usar:

```
Email: teste@teste.com
Senha: 123456
```

(Se já foi criada anteriormente)

## 📚 Tecnologias Utilizadas

- **Next.js 15** - Framework React
- **TypeScript** - Tipagem estática
- **Tailwind CSS** - Estilização
- **React Query** - Gerenciamento de estado servidor
- **Zustand** - Gerenciamento de estado cliente
- **Axios** - Cliente HTTP
- **Lucide React** - Ícones
- **Sonner** - Notificações

## 🚀 Próximos Passos

1. Implementar formulário de criação de agendamento
2. Adicionar edição de agendamentos
3. Implementar gestão de clientes
4. Adicionar visualização em calendário
5. Deploy em produção (Vercel)

## 📞 Suporte

Se tiver problemas, verifique:
1. Console do navegador (F12)
2. Network tab (requisições)
3. Backend logs

## 🎯 Checklist de Verificação

Antes de considerar o setup completo, verifique:

- [ ] `npm install` executado sem erros
- [ ] `.env.local` configurado
- [ ] Backend rodando em http://localhost:5000
- [ ] Frontend rodando em http://localhost:3000
- [ ] Consegue criar uma conta
- [ ] Consegue fazer login
- [ ] Dashboard carrega corretamente
- [ ] Pode navegar entre páginas
- [ ] Sidebar e header aparecem

Se todos os itens acima estiverem ✅, o setup está completo!

---

**Última atualização:** 16 Janeiro 2026
