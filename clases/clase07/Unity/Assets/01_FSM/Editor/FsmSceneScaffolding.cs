using UnityEngine;
using Clase07.EditorTools;
using Clase07.FSM.Demo;

namespace Clase07.FSM.EditorTools
{
    public static class FsmSceneScaffolding
    {
        public static void CreateDemoScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();

            new GameObject("WeaponDemo", typeof(FsmWeaponDemoView));
            new GameObject("GameFlowDemo", typeof(FsmGameFlowDemoView));

            SceneScaffolding.SaveScene(scene, "Assets/01_FSM/01_FSM_Demo.unity");

            var scenes = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(UnityEditor.EditorBuildSettings.scenes)
            {
                new UnityEditor.EditorBuildSettingsScene("Assets/01_FSM/01_FSM_Demo.unity", true)
            };
            UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
