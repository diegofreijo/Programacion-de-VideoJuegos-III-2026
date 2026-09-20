using UnityEngine;
using Clase07.EditorTools;
using Clase07.DI.Singleton;

namespace Clase07.DI.EditorTools
{
    public static class DiSceneScaffolding
    {
        public static void CreateSingletonScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("Bootstrapper", typeof(DiSingletonDemoBootstrapper));
            SceneScaffolding.SaveScene(scene, "Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity");
        }
    }
}
