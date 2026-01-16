# ✅ Checklist de Verificação - Semana 1

Use este checklist para verificar se tudo está funcionando corretamente.

## 📦 Instalação

### Backend
- [ ] .NET 9 SDK instalado
- [ ] Projeto AstraFuture.sln compilando sem erros
- [ ] Todas as dependências NuGet restauradas
- [ ] Arquivo `appsettings.json` configurado

### Frontend
- [ ] Node.js 18+ instalado
- [ ] Pasta `frontend/` criada
- [ ] `package.json` existe
- [ ] Todas as dependências npm instaladas

### Database
- [ ] Conta Supabase criada
- [ ] Projeto Supabase configurado
- [ ] Schema SQL executado
- [ ] Tabelas criadas corretamente

## 🔧 Configuração

### Backend
- [ ] Connection string do Supabase configurada
- [ ] JWT Secret configurado
- [ ] CORS configurado para localhost:3000
- [ ] Swagger habilitado

### Frontend
- [ ] Arquivo `.env.local` criado
- [ ] `NEXT_PUBLIC_API_URL` configurado
- [ ] TypeScript compilando sem erros
- [ ] Tailwind CSS funcionando

## 🏃‍♂️ Execução

### Backend
- [ ] `dotnet run` executa sem erros
- [ ] API responde em `http://localhost:5000`
- [ ] Swagger UI acessível em `/swagger`
- [ ] Health check endpoint responde

### Frontend
- [ ] `npm run dev` executa sem erros
- [ ] App carrega em `http://localhost:3000`
- [ ] Não há erros no console do navegador
- [ ] CSS está carregando corretamente

## 🧪 Testes Funcionais

### Autenticação

#### Register
- [ ] Página `/register` carrega
- [ ] Formulário aparece corretamente
- [ ] Validação de campos funciona
- [ ] Consegue criar nova conta
- [ ] Redireciona para dashboard após registro
- [ ] Token é salvo no localStorage

#### Login
- [ ] Página `/login` carrega
- [ ] Formulário aparece corretamente
- [ ] Validação de campos funciona
- [ ] Consegue fazer login
- [ ] Redireciona para dashboard após login
- [ ] Token é salvo no localStorage

#### Logout
- [ ] Botão de logout aparece na sidebar
- [ ] Logout limpa o localStorage
- [ ] Redireciona para página de login
- [ ] Token é removido

### Dashboard

#### Acesso
- [ ] Dashboard carrega após login
- [ ] Sidebar aparece corretamente
- [ ] Header aparece com nome do usuário
- [ ] Cards de métricas aparecem
- [ ] Navegação funciona

#### Sidebar
- [ ] Menu items aparecem
- [ ] Item ativo está destacado
- [ ] Navegação entre páginas funciona
- [ ] Email do usuário aparece
- [ ] Botão de logout funciona

### Appointments

#### Listagem
- [ ] Página `/dashboard/appointments` carrega
- [ ] Header aparece corretamente
- [ ] Botão "Novo Agendamento" aparece
- [ ] Loading state funciona
- [ ] Empty state aparece quando vazio
- [ ] Lista aparece quando há dados

#### Exclusão
- [ ] Botão de delete aparece nos cards
- [ ] Confirmação aparece ao clicar
- [ ] Delete funciona
- [ ] Toast de sucesso aparece
- [ ] Lista atualiza automaticamente

## 🔌 API Endpoints

Teste com Postman/Insomnia:

### Auth Endpoints
- [ ] `POST /api/auth/register` - Retorna 200 + token
- [ ] `POST /api/auth/login` - Retorna 200 + token
- [ ] Auth com credenciais inválidas retorna 401

### Appointments Endpoints (com token)
- [ ] `GET /api/appointments` - Retorna 200 + lista
- [ ] `POST /api/appointments` - Retorna 201 + objeto
- [ ] `GET /api/appointments/{id}` - Retorna 200 + objeto
- [ ] `PUT /api/appointments/{id}` - Retorna 200 + objeto
- [ ] `DELETE /api/appointments/{id}` - Retorna 204
- [ ] Endpoints sem token retornam 401

## 🎨 UI/UX

### Design
- [ ] Cores estão consistentes
- [ ] Fontes carregam corretamente
- [ ] Espaçamentos estão corretos
- [ ] Botões têm hover states
- [ ] Links têm hover states

### Responsividade
- [ ] Desktop (1920px) funciona
- [ ] Laptop (1366px) funciona
- [ ] Tablet (768px) funciona
- [ ] Mobile (375px) funciona
- [ ] Sidebar adapta em mobile

### Feedback
- [ ] Loading spinners aparecem
- [ ] Toasts de sucesso aparecem
- [ ] Toasts de erro aparecem
- [ ] Empty states são informativos
- [ ] Mensagens de erro são claras

## 🔐 Segurança

### Frontend
- [ ] Rotas protegidas redirecionam para login
- [ ] Token não aparece na URL
- [ ] Senha não é exibida no formulário
- [ ] HTTPS em produção (futuro)

### Backend
- [ ] Endpoints protegidos retornam 401 sem token
- [ ] Token JWT é validado
- [ ] Tenant_id é filtrado corretamente (RLS)
- [ ] Validação de dados funciona

## 📊 Performance

### Backend
- [ ] Endpoints respondem em < 500ms
- [ ] Não há N+1 queries
- [ ] Conexões ao DB são fechadas

### Frontend
- [ ] Primeira carga < 3s
- [ ] Navegação entre páginas é instantânea
- [ ] Não há memory leaks
- [ ] React Query cache funciona

## 📝 Código

### Backend
- [ ] Código compila sem warnings
- [ ] Padrão Clean Architecture seguido
- [ ] CQRS implementado corretamente
- [ ] Validações com FluentValidation funcionam
- [ ] Logs aparecem no console

### Frontend
- [ ] TypeScript sem erros
- [ ] ESLint sem warnings
- [ ] Components são reutilizáveis
- [ ] Hooks customizados funcionam
- [ ] API client funciona

## 📚 Documentação

### Arquivos de Docs
- [ ] README.md atualizado
- [ ] SETUP-FRONTEND.md existe
- [ ] SETUP-SUPABASE.md existe
- [ ] PROGRESSO-DIA-5.md existe
- [ ] ENTREGA-SEMANA-1.md existe
- [ ] PLANO-EXECUCAO.md existe

### Comentários no Código
- [ ] Funções complexas comentadas
- [ ] Interfaces documentadas
- [ ] TODOs marcados onde necessário

## 🐛 Bugs Conhecidos

Liste aqui qualquer bug encontrado durante a verificação:

1. ⬜ _Nenhum bug crítico conhecido_

---

## ✅ Resultado Final

Marque quando todos os itens acima estiverem verificados:

- [ ] **Backend 100% funcional**
- [ ] **Frontend 100% funcional**
- [ ] **Integração Backend-Frontend OK**
- [ ] **Todas as funcionalidades testadas**
- [ ] **Sem bugs críticos**
- [ ] **Documentação completa**

---

## 🚀 Próxima Ação

Quando este checklist estiver completo:

1. Commit e push de todo o código
2. Tag da release v0.1.0
3. Iniciar Dia 6 (Criar Appointment)

---

**Data de Verificação:** _________________  
**Verificado por:** _________________  
**Status:** ⏸️ Pendente | 🟡 Parcial | ✅ Completo
