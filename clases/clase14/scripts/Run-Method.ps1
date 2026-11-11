# Ejecuta un metodo estatico de Editor en batch y cierra Unity. Ejemplos:
#   ./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.Generate      # regenera datos y escenas
#   ./scripts/Run-Method.ps1 Clase14.Editor.BuildScript.BuildWindows    # lo mismo que Build.ps1
param(
    [Parameter(Mandatory)] [string]$Method,
    [string]$UnityEditorPath = ''
)

. "$PSScriptRoot/UnityEditor.ps1"
$editor = Get-UnityEditorPath $UnityEditorPath
$logPath = New-RunFile 'method' 'log'

Write-Host "Ejecutando $Method. Log: $logPath"
$exitCode = Invoke-Unity $editor @(
    '-batchmode', '-nographics', '-mute', '-quit', '-projectPath', $ProjectPath,
    '-executeMethod', $Method, '-logFile', $logPath
)
Select-String -LiteralPath $logPath -Pattern 'error CS|Exception|Clase14|Build (Succeeded|Failed)' -ErrorAction SilentlyContinue |
    Select-Object -First 40 | ForEach-Object { Write-Host $_.Line }
if ($exitCode -ne 0) {
    Write-Host "Unity termino con codigo $exitCode. Log: $logPath"
    exit 1
}
Write-Host "Listo. Log: $logPath"
exit 0
