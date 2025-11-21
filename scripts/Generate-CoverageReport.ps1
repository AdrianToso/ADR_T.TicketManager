#!/usr/bin/env pwsh

<#
.SYNOPSIS
  Genera y abre un reporte de cobertura de pruebas en formato HTML.
.DESCRIPTION
  Este script es un atajo para ejecutar el script principal de cobertura
  y abrir el informe resultante en el navegador, ignorando los umbrales de validación.
#>

# Parar en caso de error
$ErrorActionPreference = "Stop"

Write-Host "Generando y abriendo el informe de cobertura..." -ForegroundColor Cyan

# Obtener la ruta del directorio del script actual
$currentScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Construir la ruta al script principal
$mainScriptPath = Join-Path $currentScriptDir "Check-Coverage.ps1"

# Ejecutar el script principal con el parámetro -OpenReport
# Usamos 'try...finally' para asegurarnos de que el mensaje final se muestre,
# sin importar si el script principal falla por los umbrales.
try {
    # El & es el operador de llamada en PowerShell, necesario para ejecutar scripts.
    & $mainScriptPath -OpenReport
}
catch {
    # Capturamos el error si los umbrales no se cumplen, pero no hacemos nada,
    # porque el propósito de este script es solo ver el informe.
}
finally {
    $solutionDir = (Resolve-Path (Join-Path $currentScriptDir "..")).Path
    $htmlReportDir = Join-Path $solutionDir "coverage-report"
    Write-Host "Proceso finalizado. El informe está disponible en: '$htmlReportDir'" -ForegroundColor Green
}

# Salir siempre con código 0, ya que este script es solo para visualización.
exit 0
