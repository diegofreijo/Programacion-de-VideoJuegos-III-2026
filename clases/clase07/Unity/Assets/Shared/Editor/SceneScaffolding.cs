using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Clase07.EditorTools
{
    // Genera escenas mínimas desde código (-executeMethod), para no depender de
    // armar jerarquías a mano en el Editor.
    public static class SceneScaffolding
    {
        public static Scene CreateEmptyScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        public static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
