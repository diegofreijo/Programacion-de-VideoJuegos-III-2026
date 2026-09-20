using UnityEngine;
using Clase07.EditorTools;
using Clase07.MessageBroker.DIBroker;

namespace Clase07.MessageBroker.EditorTools
{
    public static class MessageBrokerSceneScaffolding
    {
        public static void CreateDiBrokerScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("LifetimeScope", typeof(DiBrokerLifetimeScope), typeof(DiBrokerDemoView));
            SceneScaffolding.SaveScene(scene, "Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity");
        }
    }
}
