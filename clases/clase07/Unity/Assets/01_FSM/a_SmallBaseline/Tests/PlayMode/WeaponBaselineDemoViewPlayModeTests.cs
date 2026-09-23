using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Clase07.FSM.Tests
{
    public class WeaponBaselineDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponBaselineScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/a_SmallBaseline/a_FSM_WeaponBaseline.unity", LoadSceneMode.Single);
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
