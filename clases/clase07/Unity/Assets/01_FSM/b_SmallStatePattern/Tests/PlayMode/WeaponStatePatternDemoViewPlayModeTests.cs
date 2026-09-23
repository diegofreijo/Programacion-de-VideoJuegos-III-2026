using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace Clase07.FSM.SmallStatePattern.Tests
{
    public class WeaponStatePatternDemoViewPlayModeTests
    {
        [UnityTest]
        public IEnumerator WeaponStatePatternScene_LoadsAndRespondsToButtons()
        {
            SceneManager.LoadScene("Assets/01_FSM/b_SmallStatePattern/b_FSM_WeaponStatePattern.unity", LoadSceneMode.Single);
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
