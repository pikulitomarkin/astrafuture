# Dockerfile para AstraFuture Frontend (Next.js)
FROM node:20-alpine AS builder
WORKDIR /app

# Copiar todo o repositório para garantir disponibilidade independentemente do build context
COPY . .

# Se o package.json estiver em frontend/, traga para a raiz (compatível com build context raiz)
RUN if [ -f package.json ]; then echo "package.json at root"; \
    elif [ -f frontend/package.json ]; then cp frontend/package.json ./ && cp frontend/package-lock.json ./ && echo "copied package files from frontend/"; \
    else echo "package.json NOT FOUND" && exit 1; fi

# Instalar dependências
RUN npm ci --prefer-offline --no-audit

# Garantir que a variável de build NEXT_PUBLIC_API_URL exista (falha explicitamente se ausente)
RUN if [ -z "$NEXT_PUBLIC_API_URL" ]; then echo "ERROR: NEXT_PUBLIC_API_URL is not set. Set it in Railway variables and clear build cache." && exit 1; else echo "NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL"; fi

# Build Next.js
RUN npm run build

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
