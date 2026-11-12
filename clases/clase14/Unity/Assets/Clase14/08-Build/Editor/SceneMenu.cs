using UnityEditor;
using UnityEditor.SceneManagement;

namespace Clase14.Editor
{
    /// <summary>Atajos para saltar de escena durante la clase: menú Clase14 > Escenas.</summary>
    static class SceneMenu
    {
        [MenuItem("Clase14/Escenas/Main (oleadas)", priority = 40)]
        static void OpenMain() => Open(SampleContent.MainScene);

        [MenuItem("Clase14/Escenas/Diálogo (Graph Toolkit en juego)", priority = 41)]
        static void OpenDialogue() => Open(SampleContent.DialogueScene);

        [MenuItem("Clase14/Escenas/Profiler (picos de GC)", priority = 42)]
        static void OpenProfiler() => Open(SampleContent.ProfilerScene);

        [MenuItem("Clase14/Escenas/Torreta (ciclo de sobrecalentamiento)", priority = 43)]
        static void OpenTurret() => Open(SampleContent.TurretScene);

        static void Open(string path)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(path);
        }
    }
}
