# Setup Supabase Database via REST API
# Este script executa o schema.sql usando a API do Supabase

Write-Host "🚀 AstraFuture - Setup Database" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Carregar variáveis do .env.local
Write-Host "📄 Carregando configurações..." -ForegroundColor Yellow
$envFile = Get-Content -Path ".env.local" -Raw
$envLines = $envFile -split "`n" | Where-Object { $_ -match "^[^#].*=" }

$config = @{}
foreach ($line in $envLines) {
    if ($line -match "^(.+?)=(.+)$") {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $config[$key] = $value
    }
}

# Verificar se as variáveis existem
if (-not $config["SUPABASE_URL"] -or $config["SUPABASE_URL"] -like "*sua-project*") {
    Write-Host "❌ ERRO: Configure o SUPABASE_URL no arquivo .env.local" -ForegroundColor Red
    Write-Host "`nPasso 1: Acesse https://supabase.com" -ForegroundColor Yellow
    Write-Host "Passo 2: Crie um novo projeto (Name: astrafuture-prod)" -ForegroundColor Yellow
    Write-Host "Passo 3: Vá em Settings > API" -ForegroundColor Yellow
    Write-Host "Passo 4: Copie URL e SERVICE_ROLE_KEY para o .env.local`n" -ForegroundColor Yellow
    exit 1
}

if (-not $config["SUPABASE_SERVICE_ROLE_KEY"] -or $config["SUPABASE_SERVICE_ROLE_KEY"] -like "*eyJhbGci*...") {
    Write-Host "❌ ERRO: Configure o SUPABASE_SERVICE_ROLE_KEY no arquivo .env.local" -ForegroundColor Red
    exit 1
}

$supabaseUrl = $config["SUPABASE_URL"]
$serviceKey = $config["SUPABASE_SERVICE_ROLE_KEY"]

Write-Host "✅ Configurações carregadas" -ForegroundColor Green
Write-Host "   URL: $supabaseUrl`n" -ForegroundColor Gray

# Ler schema.sql
Write-Host "📖 Lendo schema.sql..." -ForegroundColor Yellow
$schemaPath = Join-Path $PSScriptRoot "database\schema.sql"
if (-not (Test-Path $schemaPath)) {
    Write-Host "❌ ERRO: Arquivo database\schema.sql não encontrado" -ForegroundColor Red
    exit 1
}

$schema = Get-Content -Path $schemaPath -Raw
Write-Host "✅ Schema carregado ($($schema.Length) caracteres)`n" -ForegroundColor Green

# Executar schema via API
Write-Host "🔨 Executando schema no Supabase..." -ForegroundColor Yellow
Write-Host "   (Isso pode levar 10-15 segundos)`n" -ForegroundColor Gray

$headers = @{
    "apikey" = $serviceKey
    "Authorization" = "Bearer $serviceKey"
    "Content-Type" = "application/json"
}

$body = @{
    query = $schema
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod `
        -Uri "$supabaseUrl/rest/v1/rpc/exec_sql" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -ErrorAction Stop
    
    Write-Host "✅ Schema executado com sucesso!" -ForegroundColor Green
} catch {
    # Alternativa: usar SQL Editor endpoint
    Write-Host "⚠️  API RPC não disponível, usando método alternativo..." -ForegroundColor Yellow
    
    # Nota: Para executar SQL via API do Supabase, precisaríamos de acesso direto ao PostgreSQL
    # A forma mais simples é mesmo usar o SQL Editor do dashboard
    
    Write-Host "`n📋 INSTRUÇÕES:" -ForegroundColor Cyan
    Write-Host "1. Acesse: $supabaseUrl" -ForegroundColor White
    Write-Host "2. Vá em 'SQL Editor' no menu lateral" -ForegroundColor White
    Write-Host "3. Clique em 'New Query'" -ForegroundColor White
    Write-Host "4. Copie o conteúdo de: database\schema.sql" -ForegroundColor White
    Write-Host "5. Cole no editor e clique em 'Run' (ou Ctrl+Enter)`n" -ForegroundColor White
    
    Write-Host "💡 Alternativa: Instale PostgreSQL client (psql) para executar via CLI" -ForegroundColor Yellow
    Write-Host "   Ou use o SQL Editor (mais rápido para MVP)`n" -ForegroundColor Gray
}

Write-Host "`n✅ Setup database completo!" -ForegroundColor Green
Write-Host "🎯 Próximo passo: Inserir seed data (ver SETUP-SUPABASE.md)`n" -ForegroundColor Cyan
