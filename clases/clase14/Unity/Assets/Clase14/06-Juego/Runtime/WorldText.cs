using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Texto en el mundo con TextMesh (viene con Unity, sin paquetes). Alcanza para un HUD o una caja
    /// de diálogo de prototipo; en un juego real se usaría UI Toolkit o TextMeshPro.
    /// </summary>
    public static class WorldText
    {
        static Font s_Font;
        static Font Font => s_Font != null ? s_Font : s_Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <param name="height">Alto aproximado de una línea, en unidades del mundo.</param>
        public static TextMesh Create(Transform parent, string name, Vector3 localPosition, float height, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, int order = 100)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var text = go.AddComponent<TextMesh>();
            text.font = Font;
            text.fontSize = 64; // Resolución de la fuente; el tamaño en pantalla lo da characterSize.
            text.characterSize = height / 6.4f;
            text.anchor = anchor;
            text.color = color;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = Font.material;
            renderer.sortingOrder = order;
            return text;
        }

        /// <summary>Corta el texto en líneas de hasta <paramref name="width"/> caracteres (TextMesh no lo hace solo).</summary>
        public static string Wrap(string text, int width)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= width) return text;
            var result = new System.Text.StringBuilder();
            var lineLength = 0;
            foreach (var word in text.Split(' '))
            {
                if (lineLength > 0 && lineLength + 1 + word.Length > width)
                {
                    result.Append('\n');
                    lineLength = 0;
                }
                else if (lineLength > 0)
                {
                    result.Append(' ');
                    lineLength++;
                }
                result.Append(word);
                lineLength += word.Length;
            }
            return result.ToString();
        }
    }
}
