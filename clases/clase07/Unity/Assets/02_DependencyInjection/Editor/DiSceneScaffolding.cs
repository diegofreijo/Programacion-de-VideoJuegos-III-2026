using UnityEngine;
using Clase07.EditorTools;
using Clase07.DI.Singleton;
using Clase07.DI.ServiceLocatorPattern;

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

        public static void CreateServiceLocatorScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("Bootstrapper", typeof(DiServiceLocatorDemoBootstrapper));
            SceneScaffolding.SaveScene(scene, "Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity");
        }
    }
}
