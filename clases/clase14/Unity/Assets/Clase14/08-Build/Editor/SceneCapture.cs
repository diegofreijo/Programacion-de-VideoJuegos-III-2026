using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clase14.Editor
{
    /// <summary>
    /// Capturas de escenas sin abrir el Editor, para verificar a ojo lo que hace un script. Se corre con
    /// ./scripts/Run-Method.ps1 -Graphics Clase14.Editor.SceneCapture.CaptureOverheatedTurret
    /// (con gráficos: con -nographics no hay nada que renderizar). El PNG queda en Unity/TestResults/.
    /// </summary>
    public static class SceneCapture
    {
        /// <summary>La escena Torreta con el ciclo llevado a Recalentada (la torreta en rojo).</summary>
        public static void CaptureOverheatedTurret()
        {
            EditorSceneManager.OpenScene(SampleContent.TurretScene);
            var cycle = Object.FindAnyObjectByType<Clase14.TurretCycle>();
            cycle.Begin();
            // Diez disparos: la transición Disparos ≥ 10 lleva de Activa a Recalentada.
            for (var i = 0; i < 10; i++) cycle.Runner.RegisterShot();
            cycle.Runner.Tick(0f, enemiesInRange: true);
            cycle.Apply();
            Debug.Log($"Clase14: Estado de la torreta para la captura: {cycle.Runner.Current?.name}");

            Capture(Camera.main, "torreta-recalentada.png");
        }

        static void Capture(Camera camera, string fileName, int width = 1280, int height = 720)
        {
            var target = RenderTexture.GetTemporary(width, height, 24);
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = null;

            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);

            Directory.CreateDirectory("TestResults");
            var path = Path.GetFullPath(Path.Combine("TestResults", fileName));
            File.WriteAllBytes(path, image.EncodeToPNG());
            Object.DestroyImmediate(image);
            Debug.Log($"Clase14: captura en {path}");
        }
    }
}
