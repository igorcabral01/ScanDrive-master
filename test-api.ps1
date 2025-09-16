# Script para testar ScanDrive API no Windows
Write-Host "🧪 Testando ScanDrive API..." -ForegroundColor Green
Write-Host ""

# Verificar containers
Write-Host "📊 Verificando containers..." -ForegroundColor Yellow
docker ps | findstr scandrive

Write-Host ""
Write-Host "🔍 Testando endpoints..." -ForegroundColor Yellow
Write-Host ""

# 1. Health Check
Write-Host "1️⃣ Health Check:" -ForegroundColor Cyan
try {
    $health = Invoke-WebRequest -Uri "http://localhost/api/monitoring/health" -UseBasicParsing
    Write-Host "✅ Status: $($health.StatusCode) - $($health.StatusDescription)" -ForegroundColor Green
    Write-Host "📄 Response: $($health.Content)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro no Health Check: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 2. Swagger
Write-Host "2️⃣ Swagger UI:" -ForegroundColor Cyan
try {
    $swagger = Invoke-WebRequest -Uri "http://localhost/swagger" -UseBasicParsing
    Write-Host "✅ Swagger acessível - Status: $($swagger.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Swagger não acessível: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# 3. Testar endpoints da API
Write-Host "3️⃣ Testando outros endpoints:" -ForegroundColor Cyan

# Listar endpoints disponíveis via Swagger
Write-Host "🔍 Verificando endpoints disponíveis..."
try {
    $swaggerJson = Invoke-WebRequest -Uri "http://localhost/swagger/v1/swagger.json" -UseBasicParsing
    $swaggerData = $swaggerJson.Content | ConvertFrom-Json
    Write-Host "📋 Endpoints encontrados:" -ForegroundColor Green
    foreach ($path in $swaggerData.paths.PSObject.Properties.Name | Select-Object -First 5) {
        Write-Host "  • $path" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Não foi possível obter lista de endpoints" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 Para acessar manualmente:" -ForegroundColor Yellow
Write-Host "• API: http://localhost" -ForegroundColor White
Write-Host "• Swagger: http://localhost/swagger" -ForegroundColor White
Write-Host "• Health: http://localhost/api/monitoring/health" -ForegroundColor White

Write-Host ""
Write-Host "🌐 Abrir no navegador:" -ForegroundColor Yellow
$openBrowser = Read-Host "Deseja abrir o Swagger no navegador? (s/n)"
if ($openBrowser -eq 's' -or $openBrowser -eq 'S') {
    Start-Process "http://localhost/swagger"
}
