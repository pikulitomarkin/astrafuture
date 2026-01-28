# 🎨 Design System - Astra Agenda

## Paleta de Cores

### Cores Principais (WhatsApp Theme)

```css
/* Verde Principal - Destaque e Botões */
#25D366 - Verde WhatsApp (Botões primários, links, ícones ativos)

/* Verde Escuro - Textos e Headers */
#075E54 - Verde Escuro (Títulos, texto bold, header da sidebar)

/* Cinza Escuro - Texto Secundário */
#333333 - Cinza (Texto padrão, descrições)

/* Branco */
#FFFFFF - Branco (Fundos de cards, bubbles, backgrounds)

/* Preto */
#000000 - Preto (Contraste quando necessário)
```

### Aplicação das Cores

#### Tailwind Config
```javascript
colors: {
  whatsapp: {
    DEFAULT: '#25D366',
    primary: '#25D366',
    dark: '#075E54',
    light: '#DCF8C6',
  }
}
```

#### CSS Variables (HSL)
```css
--primary: 145 80% 42%;        /* #25D366 */
--secondary: 174 100% 23%;      /* #075E54 */
--foreground: 0 0% 20%;         /* #333333 */
--background: 0 0% 100%;        /* #FFFFFF */
```

## Componentes

### Botões

#### Tamanhos
- **sm**: `h-9 px-3 text-sm` - Botões pequenos
- **default**: `h-11 px-6 text-base` - Botões padrão (aumentado)
- **lg**: `h-12 px-8 text-base` - Botões grandes
- **xl**: `h-14 px-10 text-lg` - Botões extra grandes

#### Variantes
- **default**: Verde WhatsApp (#25D366) com hover e sombra
- **outline**: Borda verde, fundo transparente, hover verde
- **secondary**: Verde escuro (#075E54)
- **ghost**: Transparente com hover
- **link**: Texto verde com sublinhado

### Cards

- **Border**: `border-gray-200` (sutil)
- **Background**: Branco (#FFFFFF)
- **Shadow**: `shadow-md` com `hover:shadow-lg`
- **Radius**: `rounded-xl` (12px)

### Sidebar

- **Background**: Branco com sombra sutil
- **Header**: Fundo verde escuro (#075E54)
- **Logo**: Círculo verde (#25D366) com ícone de calendário
- **Item Ativo**: Fundo verde (#25D366) com texto branco e sombra
- **Item Inativo**: Texto cinza (#333333) com hover cinza claro

### Header (Páginas)

- **Background**: Branco com borda inferior
- **Títulos**: Verde escuro (#075E54) em negrito
- **Descrições**: Cinza escuro (#333333)
- **Shadow**: `shadow-sm` para profundidade

### Forms (Login/Registro)

- **Background Gradient**: De verde claro a verde escuro (10% opacidade)
- **Logo**: Círculo verde (#25D366) com ícone branco
- **Labels**: Verde escuro (#075E54) em negrito
- **Inputs**: Altura aumentada (h-11), bordas sutis
- **Links**: Verde WhatsApp (#25D366)

## Layout

### Estrutura Geral

```
┌─────────────┬────────────────────────────┐
│   Sidebar   │         Header             │
│   (fixo)    ├────────────────────────────┤
│             │                            │
│   - Logo    │      Conteúdo Principal    │
│   - Menu    │      (Cards, Listas, etc)  │
│   - User    │                            │
│   - Sair    │                            │
│             │                            │
└─────────────┴────────────────────────────┘
```

### Características

- **Sidebar**: 
  - Largura fixa: `w-64` (256px)
  - Header verde escuro com logo e nome
  - Itens com ícones e sombra quando ativos
  
- **Header**: 
  - Fixo no topo da área de conteúdo
  - Padding: `px-8 py-4`
  - Título grande e descrição menor
  
- **Conteúdo**:
  - Padding: `p-8`
  - Background: `bg-gray-50`
  - Cards com espaçamento: `gap-6`

## Tipografia

### Tamanhos

- **Títulos H1**: `text-2xl font-bold text-[#075E54]`
- **Títulos H2**: `text-xl font-semibold text-[#075E54]`
- **Cards Title**: `text-sm font-semibold text-[#333333]`
- **Números/Stats**: `text-3xl font-bold text-[#075E54]`
- **Texto Normal**: `text-base text-[#333333]`
- **Texto Pequeno**: `text-sm text-[#333333]`

### Pesos

- **Bold**: Títulos e números importantes
- **Semibold**: Labels e subtítulos
- **Medium**: Texto padrão
- **Normal**: Descrições secundárias

## Iconografia

- **Lucide React Icons**: Biblioteca padrão
- **Tamanho Padrão**: `h-5 w-5`
- **Tamanho Grande**: `h-6 w-6` ou `h-8 w-8`
- **Cores**: Verde WhatsApp para ações, Cinza para neutro

### Ícones Principais

- **Calendar**: Logo e agendamentos
- **Users**: Clientes
- **Home**: Dashboard
- **Plug**: Integrações
- **LogOut**: Sair

## Estados e Interações

### Hover

- **Botões**: Cor mais escura + sombra maior
- **Cards**: `hover:shadow-lg`
- **Links**: Sublinhado aparece
- **Menu**: Background cinza claro

### Active/Selected

- **Menu Item**: Background verde + texto branco + sombra
- **Input Focus**: Anel verde (#25D366)

### Loading

- **Skeleton**: `bg-gray-200 animate-pulse rounded`
- **Spinner**: Borda verde girando

### Badges de Status

- **Confirmado**: `bg-[#25D366]/20 text-[#075E54]` (verde suave)
- **Agendado**: `bg-blue-100 text-blue-800`
- **Cancelado**: `bg-gray-100 text-gray-800`

## Boas Práticas

1. **Consistência**: Use sempre as cores definidas, evite cores customizadas
2. **Contraste**: Verde escuro para texto importante em fundo branco
3. **Espaçamento**: Use múltiplos de 4 (4, 8, 12, 16, 24, 32px)
4. **Arredondamento**: `rounded-lg` (8px) ou `rounded-xl` (12px) para cards
5. **Sombras**: Use com moderação para hierarquia
6. **Responsividade**: Sempre teste em mobile (grid-cols-1)

## Arquivos Modificados

- ✅ `tailwind.config.js` - Cores customizadas WhatsApp
- ✅ `globals.css` - Variáveis CSS com HSL
- ✅ `button.tsx` - Tamanhos e variantes atualizadas
- ✅ `card.tsx` - Estilo aprimorado com sombras
- ✅ `sidebar.tsx` - Logo e menu com cores WhatsApp
- ✅ `header.tsx` - Cores e tipografia atualizadas
- ✅ `login/page.tsx` - Logo e gradiente
- ✅ `register/page.tsx` - Logo e gradiente
- ✅ `dashboard/page.tsx` - Cards e badges atualizados
- ✅ `appointments/page.tsx` - Botões e cores

## Screenshots de Referência

### Login/Registro
- Background: Gradiente verde suave
- Card centralizado com logo circular
- Botões grandes e verdes

### Dashboard
- Sidebar verde escuro no topo
- 3 cards de estatísticas com ícones circulares verdes
- Cards de informação com bordas sutis

### Agendamentos
- Header com título verde escuro
- Botão "Novo Agendamento" grande e verde
- Grid de cards com status coloridos
