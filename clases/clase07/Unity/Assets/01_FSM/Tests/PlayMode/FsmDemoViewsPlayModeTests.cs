using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Clase07.FSM.Tests
{
    public class FsmDemoViewsPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponStatePatternScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/02_FSM_WeaponStatePattern.unity", LoadSceneMode.Single);
            yield return null;

            yield return ClickAllButtonsAndAssertLabelsUpdated();
        }

        [UnityTest]
        public IEnumerator GameFlowScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/03_FSM_GameFlow.unity", LoadSceneMode.Single);
            yield return null;

            yield return ClickAllButtonsAndAssertLabelsUpdated();
        }

        private static IEnumerator ClickAllButtonsAndAssertLabelsUpdated()
        {
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
