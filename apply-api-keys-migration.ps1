# Script para aplicar a migração de API Keys
# Aplica a migration 004_whatsapp_integration.sql

Write-Host "=== Aplicando Migração de API Keys ===" -ForegroundColor Cyan

# Ler connection string do appsettings
$appsettingsPath = "backend-src\AstraFuture.Api\appsettings.Development.json"

if (-not (Test-Path $appsettingsPath)) {
    Write-Host "❌ Arquivo appsettings não encontrado em: $appsettingsPath" -ForegroundColor Red
    Write-Host "Por favor, execute este script da raiz do projeto (c:\astrafuture)" -ForegroundColor Yellow
    exit 1
}

$appsettings = Get-Content $appsettingsPath | ConvertFrom-Json

$connectionString = $appsettings.ConnectionStrings.DefaultConnection

if (-not $connectionString) {
    Write-Host "❌ Connection string não encontrada em appsettings" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Connection string encontrada" -ForegroundColor Green

# Parse da connection string
$connParts = @{}
$connectionString -split ';' | ForEach-Object {
    if ($_ -match '(.+?)=(.+)') {
        $connParts[$matches[1].Trim()] = $matches[2].Trim()
    }
}

$host_port = $connParts['Host'] -split ':'
$dbHost = $host_port[0]
$dbPort = if ($host_port.Length -gt 1) { $host_port[1] } else { "5432" }
$dbName = $connParts['Database']
$dbUser = $connParts['Username']
$dbPassword = $connParts['Password']

Write-Host "`nConectando ao banco de dados:" -ForegroundColor Cyan
Write-Host "  Host: $dbHost"
Write-Host "  Port: $dbPort"
Write-Host "  Database: $dbName"
Write-Host "  User: $dbUser"
Write-Host ""

# Verificar se psql está disponível
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue

if (-not $psqlPath) {
    Write-Host "❌ PostgreSQL client (psql) não encontrado no PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opções:" -ForegroundColor Yellow
    Write-Host "1. Instale PostgreSQL: https://www.postgresql.org/download/windows/" -ForegroundColor Yellow
    Write-Host "2. Ou adicione o caminho do psql ao PATH (geralmente C:\Program Files\PostgreSQL\15\bin)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Alternativa: Copie e execute o SQL manualmente:" -ForegroundColor Cyan
    Write-Host "  1. Abra pgAdmin ou outro cliente PostgreSQL"
    Write-Host "  2. Execute o arquivo: database\migrations\004_whatsapp_integration.sql"
    Write-Host ""
    exit 1
}

Write-Host "✓ PostgreSQL client encontrado: $($psqlPath.Source)" -ForegroundColor Green

# Aplicar migração
$migrationFile = "database\migrations\004_whatsapp_integration.sql"

if (-not (Test-Path $migrationFile)) {
    Write-Host "❌ Arquivo de migração não encontrado: $migrationFile" -ForegroundColor Red
    exit 1
}

Write-Host "`nAplicando migração: $migrationFile" -ForegroundColor Cyan

# Definir variável de ambiente para senha
$env:PGPASSWORD = $dbPassword

try {
    # Executar migração
    $result = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -f $migrationFile 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ Migração aplicada com sucesso!" -ForegroundColor Green
        Write-Host ""
        Write-Host "A tabela api_keys foi criada. Agora você pode:" -ForegroundColor Cyan
        Write-Host "  1. Criar API Keys na dashboard"
        Write-Host "  2. Usar as keys para autenticar o WhatsApp bot"
        Write-Host ""
    } else {
        Write-Host "`n⚠️ Migração executada com avisos:" -ForegroundColor Yellow
        Write-Host $result
        Write-Host ""
        Write-Host "Isso é normal se a tabela já existir (CREATE TABLE IF NOT EXISTS)" -ForegroundColor Gray
        Write-Host ""
    }
} catch {
    Write-Host "`n❌ Erro ao aplicar migração:" -ForegroundColor Red
    Write-Host $_.Exception.Message
    Write-Host ""
    exit 1
} finally {
    # Limpar variável de senha
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

# Verificar se tabela foi criada
Write-Host "Verificando tabela api_keys..." -ForegroundColor Cyan
$env:PGPASSWORD = $dbPassword

try {
    $checkResult = psql -h $dbHost -p $dbPort -U $dbUser -d $dbName -c "\d api_keys" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tabela api_keys existe e está pronta!" -ForegroundColor Green
        Write-Host ""
        Write-Host $checkResult
    } else {
        Write-Host "⚠️ Não foi possível verificar a tabela" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Não foi possível verificar a tabela: $($_.Exception.Message)" -ForegroundColor Yellow
} finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}

Write-Host "`n=== Concluído ===" -ForegroundColor Green
