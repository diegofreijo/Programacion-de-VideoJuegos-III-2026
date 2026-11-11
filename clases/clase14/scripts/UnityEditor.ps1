# Funciones compartidas por los scripts de clase14. Se cargan con: . "$PSScriptRoot/UnityEditor.ps1"

$ErrorActionPreference = 'Stop'
$script:ProjectPath = (Resolve-Path (Join-Path $PSScriptRoot '../Unity')).Path

function Get-UnityEditorPath([string]$UnityEditorPath) {
    $versionText = Get-Content (Join-Path $script:ProjectPath 'ProjectSettings/ProjectVersion.txt') -Raw
    $editorVersion = [regex]::Match($versionText, '(?m)^m_EditorVersion: (.+)\r?$').Groups[1].Value.Trim()
    if (-not $UnityEditorPath) { $UnityEditorPath = $env:UNITY_EDITOR_PATH }
    if (-not $UnityEditorPath) {
        $UnityEditorPath = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$editorVersion/Editor/Unity.exe"
    }
    if (-not (Test-Path -LiteralPath $UnityEditorPath -PathType Leaf)) {
        throw "No se encontro Unity $editorVersion en '$UnityEditorPath'. Instalar esa version o definir UNITY_EDITOR_PATH."
    }
    Write-Host "Unity $editorVersion, sin interfaz grafica. Cerrar este proyecto en el Editor antes de ejecutar."
    return $UnityEditorPath
}

function New-RunFile([string]$Prefix, [string]$Extension) {
    $resultsPath = Join-Path $script:ProjectPath 'TestResults'
    New-Item -ItemType Directory -Path $resultsPath -Force | Out-Null
    # Archivos unicos: nunca presentar un reporte viejo como resultado de esta corrida.
    $runId = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
    return Join-Path $resultsPath "$Prefix-$runId.$Extension"
}

function Invoke-Unity([string]$EditorPath, [string[]]$EditorArgs) {
    # Out-Host mantiene PowerShell esperando al ejecutable en modo batch.
    & $EditorPath @EditorArgs | Out-Host
    return $LASTEXITCODE
}
