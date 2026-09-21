using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Clase07.Shared.UI;

namespace Clase07.Shared.Tests
{
    public class DemoUiFactoryTests
    {
        [TearDown]
        public void TearDown()
        {
            foreach (var go in Object.FindObjectsOfType<GameObject>())
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CreateCanvas_CreatesExactlyOneEventSystem_EvenWhenCalledTwice()
        {
            DemoUiFactory.CreateCanvas();
            DemoUiFactory.CreateCanvas();

            Assert.AreEqual(1, Object.FindObjectsOfType<EventSystem>().Length);
        }

        [Test]
        public void CreateButton_SetsLabelText()
        {
            var canvas = DemoUiFactory.CreateCanvas();

            var button = DemoUiFactory.CreateButton(canvas.transform, "Fire", Vector2.zero);

            var text = button.GetComponentInChildren<UnityEngine.UI.Text>();
            Assert.AreEqual("Fire", text.text);
        }

        [Test]
        public void CreateInputField_HasTextComponentAssigned()
        {
            var canvas = DemoUiFactory.CreateCanvas();

            var inputField = DemoUiFactory.CreateInputField(canvas.transform, Vector2.zero);

            Assert.IsNotNull(inputField.textComponent);
        }
    }
}
