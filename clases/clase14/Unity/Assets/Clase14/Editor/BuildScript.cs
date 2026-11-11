using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Clase14.Editor
{
    /// <summary>
    /// Build por línea de comandos. Lo llama scripts/Build.ps1 con
    /// -batchmode -executeMethod Clase14.Editor.BuildScript.BuildWindows
    /// </summary>
    public static class BuildScript
    {
        public static void BuildWindows()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = "Build/Windows/Clase14.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"Build {summary.result}: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:0} s, " +
                      $"{summary.totalErrors} errores, {summary.totalWarnings} advertencias.");

            // En batch, el código de salida le dice al script (o a CI, o a un agente) si salió bien.
            if (Application.isBatchMode)
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
