using UnityEngine;
using UnityEditor;
using Clase07.EditorTools;
using Clase07.MessageBroker.DIBroker;
using Clase07.MessageBroker.ScriptableObjectChannels;
using Clase07.MessageBroker.MessagePipeExample;

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

        public static void CreateScriptableObjectChannelsScene()
        {
            var channel = ScriptableObject.CreateInstance<ScoreEventChannelSO>();
            AssetDatabase.CreateAsset(channel, "Assets/03_MessageBroker/02_ScriptableObjectChannels/ScoreEventChannel.asset");
            AssetDatabase.SaveAssets();

            var scene = SceneScaffolding.CreateEmptyScene();
            var view = new GameObject("DemoView", typeof(ScoreEventChannelsDemoView)).GetComponent<ScoreEventChannelsDemoView>();
            var serializedView = new SerializedObject(view);
            serializedView.FindProperty("_channel").objectReferenceValue = channel;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            SceneScaffolding.SaveScene(scene, "Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity");
        }

        public static void CreateMessagePipeScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("LifetimeScope", typeof(MessagePipeLifetimeScope), typeof(MessagePipeDemoView));
            SceneScaffolding.SaveScene(scene, "Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity");
        }
    }
}
