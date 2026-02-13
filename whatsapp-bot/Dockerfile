FROM python:3.11-slim

WORKDIR /app

# Instalar dependências do sistema
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# Copiar requirements
COPY requirements.txt .

# Instalar dependências Python
RUN pip install --no-cache-dir -r requirements.txt

# Copiar código fonte
COPY src/ ./src/

# Variáveis de ambiente
ENV PYTHONUNBUFFERED=1
ENV PYTHONPATH=/app
ENV PORT=5000

# Expor porta
EXPOSE 5000

# Script de inicialização para usar PORT dinâmico
CMD gunicorn --bind 0.0.0.0:${PORT:-5000} --workers 2 --timeout 120 --access-logfile - --error-logfile - src.bot:app
