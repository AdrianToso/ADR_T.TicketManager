<#
.SYNOPSIS
    Script de inicialización para el ambiente Docker de ADR_T.TicketManager
.DESCRIPTION
    Este script automatiza la configuración y puesta en marcha del ambiente de desarrollo
    con Docker Compose, incluyendo SQL Server, RabbitMQ y los servicios de la aplicación.
#>

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " INICIALIZACIÓN AMBIENTE DOCKER" -ForegroundColor Cyan
Write-Host " ADR_T.TicketManager" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Función para verificar si un comando existe
function Test-CommandExists {
    param($command)
    $exists = $null -ne (Get-Command $command -ErrorAction SilentlyContinue)
    return $exists
}

# Verificar que Docker esté instalado y funcionando
Write-Host "`n[1/5] Verificando prerequisitos..." -ForegroundColor Yellow

if (-not (Test-CommandExists "docker")) {
    Write-Host "ERROR: Docker no está instalado o no está en el PATH." -ForegroundColor Red
    Write-Host "Por favor instala Docker Desktop desde: https://www.docker.com/products/docker-desktop/" -ForegroundColor Red
    exit 1
}

if (-not (Test-CommandExists "docker-compose")) {
    Write-Host "ERROR: Docker Compose no está disponible." -ForegroundColor Red
    Write-Host "Asegúrate de que Docker Desktop esté correctamente instalado." -ForegroundColor Red
    exit 1
}

# Verificar que Docker esté ejecutándose
try {
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Docker no está ejecutándose"
    }
} catch {
    Write-Host "ERROR: Docker no está ejecutándose." -ForegroundColor Red
    Write-Host "Inicia Docker Desktop y espera a que esté listo." -ForegroundColor Red
    exit 1
}

Write-Host "✓ Docker y Docker Compose verificados" -ForegroundColor Green

# Verificar y crear archivo .env si no existe
Write-Host "`n[2/5] Configurando variables de entorno..." -ForegroundColor Yellow

$envExamplePath = ".env.ejemplo"
$envPath = ".env"

if (-not (Test-Path $envPath)) {
    if (Test-Path $envExamplePath) {
        Copy-Item $envExamplePath $envPath
        Write-Host "✓ Archivo .env creado desde .env.ejemplo" -ForegroundColor Green
        Write-Host "  Por favor revisa el archivo .env y ajusta las variables si es necesario" -ForegroundColor Yellow
    } else {
        Write-Host "ERROR: No se encuentra .env.ejemplo" -ForegroundColor Red
        Write-Host "Asegúrate de que el archivo .env.ejemplo exista en el directorio raíz." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✓ Archivo .env ya existe" -ForegroundColor Green
}

# Construir y levantar los contenedores
Write-Host "`n[3/5] Construyendo y levantando contenedores..." -ForegroundColor Yellow
Write-Host "Esto puede tomar varios minutos en la primera ejecución..." -ForegroundColor Yellow

try {
    # Limpiar contenedores previos para evitar conflictos
    Write-Host "Limpiando contenedores previos..." -ForegroundColor Gray
    docker-compose down --remove-orphans
    
    # Construir y levantar
    Write-Host "Construyendo imágenes..." -ForegroundColor Gray
    docker-compose build --no-cache
    
    Write-Host "Iniciando servicios..." -ForegroundColor Gray
    docker-compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error al ejecutar docker-compose up"
    }
    
    Write-Host "✓ Contenedores construidos y ejecutándose" -ForegroundColor Green
} catch {
    Write-Host "ERROR: Fallo al construir o ejecutar los contenedores: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Revisa los logs con: docker-compose logs" -ForegroundColor Yellow
    exit 1
}

# Esperar a que los servicios estén saludables
Write-Host "`n[4/5] Esperando a que los servicios estén listos..." -ForegroundColor Yellow

$timeout = 180  # 3 minutos timeout
$elapsed = 0
$interval = 10  # Verificar cada 10 segundos

do {
    Start-Sleep -Seconds $interval
    $elapsed += $interval
    
    $sqlHealth = docker inspect --format='{{.State.Health.Status}}' servidor-sql-ticketmanager 2>&1
    $rabbitHealth = docker inspect --format='{{.State.Health.Status}}' rabbitmq-ticketmanager 2>&1
    
    Write-Host "Esperando servicios... ($elapsed/$timeout segundos)" -ForegroundColor Gray
    
    if ($elapsed -ge $timeout) {
        Write-Host "TIMEOUT: Los servicios no están listos después de $timeout segundos" -ForegroundColor Red
        Write-Host "Revisa los logs con: docker-compose logs" -ForegroundColor Yellow
        break
    }
} while ($sqlHealth -ne "healthy" -or $rabbitHealth -ne "healthy")

if ($sqlHealth -eq "healthy" -and $rabbitHealth -eq "healthy") {
    Write-Host "✓ Todos los servicios están saludables" -ForegroundColor Green
}

# Mostrar información de acceso
Write-Host "`n[5/5] Ambiente inicializado exitosamente!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " SERVICIOS DISPONIBLES:" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "• API TicketManager:   http://localhost:5000" -ForegroundColor White
Write-Host "• Servicio Notificaciones: http://localhost:5070" -ForegroundColor White
Write-Host "• RabbitMQ Management: http://localhost:15672" -ForegroundColor White
Write-Host "• SQL Server:          localhost,1433" -ForegroundColor White
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " CREDENCIALES:" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "• SQL Server:" -ForegroundColor White
Write-Host "  Usuario: sa" -ForegroundColor Gray
Write-Host "  Contraseña: [Revisa tu archivo .env]" -ForegroundColor Gray
Write-Host "• RabbitMQ:" -ForegroundColor White
Write-Host "  Usuario: admin" -ForegroundColor Gray
Write-Host "  Contraseña: [Revisa tu archivo .env]" -ForegroundColor Gray
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " COMANDOS ÚTILES:" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "• Ver logs:            docker-compose logs" -ForegroundColor Gray
Write-Host "• Ver logs específicos: docker-compose logs [servicio]" -ForegroundColor Gray
Write-Host "• Detener servicios:   docker-compose down" -ForegroundColor Gray
Write-Host "• Reiniciar servicios: docker-compose restart" -ForegroundColor Gray
Write-Host "==========================================" -ForegroundColor Cyan

# Verificación final opcional
$response = Read-Host "`n¿Deseas ver el estado de los contenedores? (s/n)"
if ($response -eq 's' -or $response -eq 'S') {
    docker-compose ps
}