# Hook de Pre-Commit para validar la cobertura de código por capas y generar un informe HTML.

param (
    [switch]$OpenReport
)

# --- CONFIGURACIÓN DE UMBRALES ---
$coreThreshold = 90
$applicationThreshold = 80
$infrastructureThreshold = 0
# ---------------------------------

Write-Host "Ejecutando script de cobertura por capas..."
Write-Host "  -> Ejecutando SOLO pruebas unitarias..." -ForegroundColor Green

# Definir rutas basadas en la ubicación del script
try {
    $scriptPath = $MyInvocation.MyCommand.Path
    $solutionDir = (Resolve-Path (Join-Path $scriptPath "../..")).Path 
} catch {
    Write-Host "Error: No se pudo determinar la ruta de la solución." -ForegroundColor Red
    exit 1
}

$testResultsDir = Join-Path $solutionDir "TestResults"
$htmlReportDir = Join-Path $solutionDir "coverage-report"

Write-Host "  -> Paso 1: Ejecutando pruebas y generando informe..."
if (Test-Path $testResultsDir) { Remove-Item -Recurse -Force $testResultsDir }
New-Item -ItemType Directory -Path $testResultsDir | Out-Null

# EJECUTAR SOLO EL PROYECTO DE PRUEBAS UNITARIAS
$unitTestProject = Join-Path $solutionDir "ADR_T.TicketManager.Tests\ADR_T.TicketManager.Tests.csproj"

if (-not (Test-Path $unitTestProject)) {
    Write-Host "  -> ERROR: No se encontró el proyecto de pruebas unitarias." -ForegroundColor Red
    exit 1
}

Write-Host "  -> Ejecutando pruebas en: $unitTestProject" -ForegroundColor Gray

$testOutput = dotnet test "$unitTestProject" `
    --settings "$solutionDir/solution.runsettings" `
    --collect:"XPlat Code Coverage;Format=cobertura" `
    --results-directory "$testResultsDir" `
    --logger "console;verbosity=normal" | Out-String

if ($LASTEXITCODE -ne 0) {
    Write-Host "  -> ERROR: Las pruebas unitarias fallaron." -ForegroundColor Red
    exit 1
}
Write-Host "  -> OK: Pruebas unitarias ejecutadas con éxito."

Write-Host "  -> Paso 2: Buscando el informe de cobertura..."
$coverageReportPath = $null

$coverageReportPath = Get-ChildItem -Path "$testResultsDir" -Filter "coverage.cobertura.xml" -Recurse | Select-Object -ExpandProperty FullName -First 1

if ($null -eq $coverageReportPath -or -not (Test-Path $coverageReportPath)) {
    Write-Host "  -> ERROR: No se encontró el archivo 'coverage.cobertura.xml' dentro de '$testResultsDir'. Revise que Coverlet se haya ejecutado correctamente." -ForegroundColor Red
    exit 1
}
Write-Host "  -> OK: Informe XML encontrado en '$coverageReportPath'."

[xml]$coverageXml = Get-Content $coverageReportPath

function Get-CoverageForAssembly($assemblyName, $xmlReport) {
    $package = $xmlReport.coverage.packages.package | Where-Object { $_.name -eq $assemblyName }
    if ($null -ne $package) {
        return [Math]::Round(([double]$package.'line-rate' * 100), 2)
    }
    return 0
}

Write-Host "  -> Paso 3: Validando umbrales de cobertura..."

$coreCoverage = Get-CoverageForAssembly "ADR_T.TicketManager.Core" $coverageXml
$applicationCoverage = Get-CoverageForAssembly "ADR_T.TicketManager.Application" $coverageXml 
$infrastructureCoverage = Get-CoverageForAssembly "ADR_T.TicketManager.Infrastructure" $coverageXml 

Write-Host "    - Core: $coreCoverage% (Requerido: $coreThreshold%)"
Write-Host "    - Application: $applicationCoverage% (Requerido: $applicationThreshold%)"
Write-Host "    - Infrastructure: $infrastructureCoverage% (Requerido: $infrastructureThreshold%)"

$commitAllowed = $true
if ($coreCoverage -lt $coreThreshold) { $commitAllowed = $false }
if ($applicationCoverage -lt $applicationThreshold) { $commitAllowed = $false }
if ($infrastructureCoverage -lt $infrastructureThreshold) { $commitAllowed = $false }

Write-Host "  -> Paso 4: Generando informe HTML de cobertura..."
try {
    if (Test-Path $htmlReportDir) { Remove-Item -Recurse -Force $htmlReportDir }
    
    $reportGeneratorArgs = @(
        "-reports:$coverageReportPath",
        "-targetdir:$htmlReportDir",
        "-reporttypes:Html"
    )
    
    & dotnet tool run reportgenerator -- $reportGeneratorArgs 2>$null

    Write-Host "  -> OK: Informe HTML generado en la carpeta '$htmlReportDir'." -ForegroundColor Green
} catch {
    Write-Host "  -> ERROR: Falló la generación del informe HTML." -ForegroundColor Red
    Write-Host "    Asegúrese de que el manifiesto (.config/dotnet-tools.json) sea correcto y ejecute 'dotnet tool restore'."
}

if ($OpenReport) {
    $reportIndexPath = Join-Path $htmlReportDir "index.html"
    if (Test-Path $reportIndexPath) {
        Write-Host "  -> Abriendo el informe en el navegador..."
        Start-Process -FilePath $reportIndexPath
    } else {
        Write-Host "  -> ADVERTENCIA: No se encontró 'index.html' para abrir." -ForegroundColor Yellow
    }
}

if ($commitAllowed) {
    Write-Host "ÉXITO: Todos los umbrales de cobertura se cumplen." -ForegroundColor Green
    exit 0
} else {
    Write-Host "FALLÓ: Uno o más umbrales de cobertura no se cumplen. Revisa el informe en '$htmlReportDir'." -ForegroundColor Red
    exit 1
}