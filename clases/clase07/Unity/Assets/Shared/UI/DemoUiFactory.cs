using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clase07.Shared.UI
{
    // Utilidades para armar UI de demo 100% por código: nada de wiring hecho a
    // mano en el Inspector, así cada escena se genera y se verifica sin abrir
    // el Editor de forma interactiva.
    public static class DemoUiFactory
    {
        public static Canvas CreateCanvas(string name = "Canvas")
        {
            var canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return canvas;
        }

        public static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition)
        {
            var buttonGo = new GameObject($"Button_{label}", typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(parent, false);
            var rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180, 30);
            rect.anchoredPosition = anchoredPosition;

            var text = CreateChildText(buttonGo.transform, label, TextAnchor.MiddleCenter);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGo.GetComponent<Button>();
        }

        public static Text CreateLabel(Transform parent, string initialText, Vector2 anchoredPosition)
        {
            var text = CreateChildText(parent, initialText, TextAnchor.MiddleLeft);
            text.transform.SetParent(parent, false);
            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 30);
            rect.anchoredPosition = anchoredPosition;
            return text;
        }

        public static InputField CreateInputField(Transform parent, Vector2 anchoredPosition)
        {
            var go = new GameObject("InputField", typeof(Image), typeof(InputField));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180, 30);
            rect.anchoredPosition = anchoredPosition;

            var inputField = go.GetComponent<InputField>();
            var text = CreateChildText(go.transform, string.Empty, TextAnchor.MiddleLeft);
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 4);
            textRect.offsetMax = new Vector2(-8, -4);
            inputField.textComponent = text;

            return inputField;
        }

        private static Text CreateChildText(Transform parent, string content, TextAnchor alignment)
        {
            var textGo = new GameObject("Text", typeof(Text));
            textGo.transform.SetParent(parent, false);
            var text = textGo.GetComponent<Text>();
            text.text = content;
            text.alignment = alignment;
            text.color = Color.black;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }
    }
}
