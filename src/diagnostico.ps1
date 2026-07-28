param(
    [int]$LogTimeoutSeconds = 15
)

$ErrorActionPreference = "Stop"
$ROOT = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTICO DE CONTENEDORES .NET"       -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ─────────────────────────────────────────────
# PASO 1: Verificar puertos
# ─────────────────────────────────────────────
Write-Host "[1/4] Verificando puertos..." -ForegroundColor Yellow
$puertos = @{
    5433 = "catalogdb (host->5432)"
    5434 = "basketdb (host->5432)"
    6379 = "distributedcache (Redis)"
    6000 = "catalog.api HTTP"
    6060 = "catalog.api HTTPS"
    6001 = "basket.api HTTP"
    6061 = "basket.api HTTPS"
}

$conflictos = $false
foreach ($port in $puertos.Keys) {
    $result = netstat -ano | Select-String ":$port\s"
    if ($result) {
        $conflictos = $true
        $procid = ($result -split '\s+')[-1]
        $proc = Get-Process -Id $procid -ErrorAction SilentlyContinue
        Write-Host "  [!] Puerto $port ($($puertos[$port])) OCUPADO por PID $procid ($($proc.ProcessName))" -ForegroundColor Red
    } else {
        Write-Host "  [OK] Puerto $port ($($puertos[$port])) libre" -ForegroundColor Green
    }
}

if ($conflictos) {
    Write-Host ""
    Write-Host "ADVERTENCIA: Hay puertos ocupados. Se usaran puertos alternativos." -ForegroundColor Magenta
}

# ─────────────────────────────────────────────
# PASO 2: Generar override con correcciones
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Generando override temporal con correcciones..." -ForegroundColor Yellow

@"
services:
  catalog.api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_HTTP_PORTS: "8080"
      ConnectionStrings__CatalogDb: "Host=catalogdb;Port=5432;Database=CatalogDb;Username=postgres;Password=postgres"
    ports:
      - "6100:8080"

  basket.api:
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_HTTP_PORTS: "8080"
      ConnectionStrings__Database: "Host=basketdb;Port=5432;Database=BasketDb;Username=postgres;Password=postgres;Include Error Detail=true"
      ConnectionStrings__Redis: "distributedcache:6379"
    ports:
      - "6101:8080"
"@ | Set-Content -Path (Join-Path $ROOT "docker-compose.diagnostico.yml") -Encoding UTF8

Write-Host "  Override generado en docker-compose.diagnostico.yml" -ForegroundColor Green

# ─────────────────────────────────────────────
# PASO 3: Construir y levantar
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "[3/4] Construyendo imagenes..." -ForegroundColor Yellow

docker-compose -f "$ROOT\docker-compose.yml" -f "$ROOT\docker-compose.override.yml" -f "$ROOT\docker-compose.diagnostico.yml" build 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Build fallo. Vea el mensaje arriba." -ForegroundColor Red
    Write-Host "  Continuando con contenedores existentes..." -ForegroundColor DarkYellow
}

Write-Host "  Levantando contenedores..." -ForegroundColor Yellow
docker-compose -f "$ROOT\docker-compose.yml" -f "$ROOT\docker-compose.override.yml" -f "$ROOT\docker-compose.diagnostico.yml" up -d 2>&1
Start-Sleep -Seconds 5

# ─────────────────────────────────────────────
# PASO 4: Ver logs y probar
# ─────────────────────────────────────────────
Write-Host ""
Write-Host "[4/4] Resumen de estado..." -ForegroundColor Yellow

docker-compose -f "$ROOT\docker-compose.yml" -f "$ROOT\docker-compose.override.yml" -f "$ROOT\docker-compose.diagnostico.yml" ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}" 2>&1

Write-Host ""
Write-Host "--- LOGS DE CATALOG.API ---" -ForegroundColor Cyan
docker-compose -f "$ROOT\docker-compose.yml" -f "$ROOT\docker-compose.override.yml" -f "$ROOT\docker-compose.diagnostico.yml" logs --tail=15 catalog.api 2>&1

Write-Host ""
Write-Host "--- LOGS DE BASKET.API ---" -ForegroundColor Cyan
docker-compose -f "$ROOT\docker-compose.yml" -f "$ROOT\docker-compose.override.yml" -f "$ROOT\docker-compose.diagnostico.yml" logs --tail=15 basket.api 2>&1

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTICO COMPLETADO"                 -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para probar los endpoints:" -ForegroundColor Green
Write-Host '  curl http://localhost:6000/products?pageNumber=1' -ForegroundColor White
Write-Host '  curl http://localhost:6001/basket/testuser' -ForegroundColor White
Write-Host ""
Write-Host "Para ver logs en tiempo real:" -ForegroundColor Green
Write-Host '  docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.diagnostico.yml logs -f' -ForegroundColor White
Write-Host ""
Write-Host "Para limpiar:" -ForegroundColor Green
Write-Host '  docker-compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.diagnostico.yml down' -ForegroundColor White
