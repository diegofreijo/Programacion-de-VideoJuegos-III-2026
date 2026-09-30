param(
    [string]$TestFilter = '',
    [string]$UnityEditorPath = $env:UNITY_EDITOR_PATH
)

$ErrorActionPreference = 'Stop'
$projectPath = (Resolve-Path (Join-Path $PSScriptRoot '../Unity')).Path
$versionText = Get-Content (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt') -Raw
$editorVersion = [regex]::Match($versionText, '(?m)^m_EditorVersion: (.+)\r?$').Groups[1].Value.Trim()

if (-not $UnityEditorPath) {
    $UnityEditorPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$editorVersion/Editor/Unity.exe"
}
if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
    throw "No se encontro Unity $editorVersion en '$UnityEditorPath'. Instalar esa version o definir UNITY_EDITOR_PATH con la ruta completa a Unity.exe antes de abrir VS Code."
}

$resultsPath = Join-Path $projectPath 'TestResults'
New-Item -ItemType Directory -Path $resultsPath -Force | Out-Null
# Archivos unicos: nunca presentar un reporte viejo como resultado de esta corrida.
$runId = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$reportPath = Join-Path $resultsPath "editmode-$runId.xml"
$logPath = Join-Path $resultsPath "editmode-$runId.log"
$editorArgs = @(
    '-batchmode', '-nographics', '-projectPath', $projectPath,
    '-runTests', '-testPlatform', 'EditMode',
    '-testResults', $reportPath, '-logFile', $logPath
)
if ($TestFilter) { $editorArgs += @('-testFilter', $TestFilter) }

Write-Host "Ejecutando Edit Mode con Unity $editorVersion, sin interfaz grafica."
Write-Host 'Cerrar este proyecto en Unity antes de ejecutar. No iniciar dos tareas simultaneamente.'
Write-Host "Log: $logPath"
# No agregar -quit: el Test Runner cierra Unity cuando termina de escribir el XML.
# Out-Host mantiene PowerShell esperando al ejecutable grafico en modo batch.
& $UnityEditorPath @editorArgs | Out-Host
$editorExitCode = $LASTEXITCODE

if (-not (Test-Path -LiteralPath $reportPath)) {
    Get-Content -LiteralPath $logPath -Tail 50 -ErrorAction SilentlyContinue
    throw "Unity termino sin reporte (codigo $editorExitCode). Revisar compilacion, licencia o proyecto abierto. Log: $logPath"
}
[xml]$report = Get-Content -LiteralPath $reportPath -Raw
$run = $report.'test-run'
Write-Host "Resultado: $($run.result). Total: $($run.total). Aprobados: $($run.passed). Fallidos: $($run.failed)."
foreach ($test in $report.SelectNodes('//test-case')) {
    Write-Host "[$($test.result)] $($test.fullname)"
    if ($test.failure) { Write-Host $test.failure.message.InnerText }
}
Write-Host "Reporte NUnit: $reportPath"
if ($editorExitCode -ne 0 -or $run.result -ne 'Passed' -or [int]$run.total -eq 0) {
    exit 1
}
exit 0
