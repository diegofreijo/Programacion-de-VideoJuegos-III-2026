using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Clase07.FSM.LargeScriptableObjectStates.Tests
{
    public class GameFlowSoDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator GameFlowScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/d_LargeScriptableObjectStates/d_FSM_GameFlow.unity", LoadSceneMode.Single);
            yield return null;

            var buttons = Object.FindObjectsOfType<Button>();
            Assert.Greater(buttons.Length, 0);

            foreach (var button in buttons)
            {
                button.onClick.Invoke();
            }
            yield return null;

            var labels = Object.FindObjectsOfType<TMP_Text>();
            Assert.Greater(labels.Length, 0);
        }
    }
}
