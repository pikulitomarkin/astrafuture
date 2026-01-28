# Dockerfile para AstraFuture Frontend (Next.js)
FROM node:20-alpine AS builder
WORKDIR /app

# Copiar package files explicitamente
COPY package.json ./
COPY package-lock.json ./

# Instalar dependências
RUN npm ci --prefer-offline --no-audit

# Copiar código fonte
COPY . .

# Build Next.js
RUN npm run build

FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV PORT=3000

# Copiar arquivos buildados do Next.js standalone
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static

EXPOSE 3000

CMD ["node", "server.js"]
