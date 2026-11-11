# Corre los tests de clase14 en batch, sin abrir el Editor.
#   ./scripts/Test.ps1                       # Edit Mode
#   ./scripts/Test.ps1 -Platform PlayMode    # Play Mode
#   ./scripts/Test.ps1 -TestFilter Clase14.Tests.EditMode.EnemyDefinitionTests
param(
    [ValidateSet('EditMode', 'PlayMode')]
    [string]$Platform = 'EditMode',
    [string]$TestFilter = '',
    [string]$UnityEditorPath = ''
)

. "$PSScriptRoot/UnityEditor.ps1"
$editor = Get-UnityEditorPath $UnityEditorPath
$reportPath = New-RunFile $Platform.ToLower() 'xml'
$logPath = [IO.Path]::ChangeExtension($reportPath, 'log')

$editorArgs = @(
    '-batchmode', '-nographics', '-mute', '-projectPath', $ProjectPath,
    '-runTests', '-testPlatform', $Platform,
    '-testResults', $reportPath, '-logFile', $logPath
)
if ($TestFilter) { $editorArgs += @('-testFilter', $TestFilter) }

Write-Host "Tests $Platform. Log: $logPath"
# Sin -quit: el Test Runner cierra Unity cuando termina de escribir el XML.
$exitCode = Invoke-Unity $editor $editorArgs

if (-not (Test-Path -LiteralPath $reportPath)) {
    Get-Content -LiteralPath $logPath -Tail 60 -ErrorAction SilentlyContinue
    throw "Unity termino sin reporte (codigo $exitCode). Revisar errores de compilacion en el log: $logPath"
}
[xml]$report = Get-Content -LiteralPath $reportPath -Raw
$run = $report.'test-run'
Write-Host "Resultado: $($run.result). Total: $($run.total). Aprobados: $($run.passed). Fallidos: $($run.failed)."
foreach ($test in $report.SelectNodes('//test-case')) {
    Write-Host "[$($test.result)] $($test.fullname)"
    if ($test.failure) { Write-Host "    $($test.failure.message.InnerText)" }
}
Write-Host "Reporte NUnit: $reportPath"
if ($exitCode -ne 0 -or $run.result -ne 'Passed' -or [int]$run.total -eq 0) { exit 1 }
exit 0
