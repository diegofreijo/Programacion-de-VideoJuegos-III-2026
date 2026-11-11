# Build de Windows por linea de comandos. El ejecutable queda en Unity/Build/Windows/Clase14.exe
param([string]$UnityEditorPath = '')

& "$PSScriptRoot/Run-Method.ps1" -Method 'Clase14.Editor.BuildScript.BuildWindows' -UnityEditorPath $UnityEditorPath
exit $LASTEXITCODE
