using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Clase07.FSM.Tests
{
    public class FsmDemoViewsPlayModeTests
    {
        [UnityTest]
        public IEnumerator FsmDemoScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/01_FSM_Demo.unity", LoadSceneMode.Single);
            yield return null;

            var buttons = Object.FindObjectsOfType<Button>();
            Assert.Greater(buttons.Length, 0);

            foreach (var button in buttons)
            {
                button.onClick.Invoke();
            }
            yield return null;

            var labels = Object.FindObjectsOfType<Text>();
            Assert.Greater(labels.Length, 0);
        }
    }
}
