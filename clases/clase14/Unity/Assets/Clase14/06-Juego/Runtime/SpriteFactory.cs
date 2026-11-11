using UnityEngine;

namespace Clase14
{
    /// <summary>Sprites generados por código: el proyecto no necesita arte para la demo.</summary>
    public static class SpriteFactory
    {
        static Sprite s_Square;
        static Sprite s_Circle;

        public static Sprite Square => s_Square != null ? s_Square : s_Square = Create(false);
        public static Sprite Circle => s_Circle != null ? s_Circle : s_Circle = Create(true);

        static Sprite Create(bool circle)
        {
            const int size = 32;
            var texture = new Texture2D(size, size) { filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var inside = !circle || new Vector2(x - center, y - center).magnitude <= size * 0.5f;
                texture.SetPixel(x, y, inside ? Color.white : Color.clear);
            }
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        public static SpriteRenderer AddRenderer(GameObject go, Sprite sprite, Color color, int order = 0)
        {
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
