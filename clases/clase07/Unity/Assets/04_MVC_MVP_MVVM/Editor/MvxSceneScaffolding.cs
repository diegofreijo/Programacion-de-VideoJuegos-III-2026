using UnityEngine;
using Clase07.EditorTools;
using Clase07.Mvx.Mvc;
using Clase07.Mvx.Mvp;

namespace Clase07.Mvx.EditorTools
{
    public static class MvxSceneScaffolding
    {
        public static void CreateMvcScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("InventoryController", typeof(InventoryController));
            SceneScaffolding.SaveScene(scene, "Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity");
        }

        public static void CreateMvpScene()
        {
            var scene = SceneScaffolding.CreateEmptyScene();
            new GameObject("InventoryMvpView", typeof(InventoryMvpView));
            SceneScaffolding.SaveScene(scene, "Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity");
        }
    }
}
