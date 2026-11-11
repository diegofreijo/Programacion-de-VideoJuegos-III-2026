using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// La caja de diálogo en pantalla: quién habla, qué dice y las opciones. Solo dibuja; quién
    /// decide qué línea mostrar es DialogueNpc.
    /// </summary>
    public class DialogueBox : MonoBehaviour
    {
        [Tooltip("Ancho y alto de la caja, en unidades del mundo.")]
        public Vector2 size = new Vector2(15f, 3.4f);
        [Tooltip("Caracteres por línea antes de cortar el texto.")]
        [Min(10)] public int wrapWidth = 55;

        GameObject m_Panel;
        TextMesh m_Speaker, m_Body, m_Choices;

        public bool IsVisible => m_Panel != null && m_Panel.activeSelf;
        public string Speaker => m_Speaker != null ? m_Speaker.text : "";
        public string Body => m_Body != null ? m_Body.text : "";
        public string Choices => m_Choices != null ? m_Choices.text : "";

        void Awake()
        {
            Build();
            Hide();
        }

        /// <summary>Muestra una línea del diálogo. Si tiene opciones, las numera para elegir con 1, 2…</summary>
        public void Show(DialogueAsset.Line line)
        {
            Build();
            m_Panel.SetActive(true);
            m_Speaker.text = line.speaker;
            m_Body.text = WorldText.Wrap(line.text, wrapWidth);
            if (line.choices.Count == 0)
            {
                m_Choices.text = "[Espacio] seguir";
                return;
            }
            var choices = new System.Text.StringBuilder();
            for (var i = 0; i < line.choices.Count; i++)
                choices.Append($"[{i + 1}] {line.choices[i].text}   ");
            m_Choices.text = choices.ToString();
        }

        public void Hide()
        {
            Build();
            m_Panel.SetActive(false);
        }

        // Los hijos se crean por código (como los sprites): la escena solo guarda este componente.
        void Build()
        {
            if (m_Panel != null) return;
            m_Panel = new GameObject("Panel");
            m_Panel.transform.SetParent(transform, false);

            var background = new GameObject("Fondo");
            background.transform.SetParent(m_Panel.transform, false);
            background.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteFactory.AddRenderer(background, SpriteFactory.Square, new Color(0.05f, 0.06f, 0.1f, 0.92f), 90);

            var left = -size.x * 0.5f + 0.4f;
            var top = size.y * 0.5f - 0.3f;
            m_Speaker = WorldText.Create(m_Panel.transform, "Quién habla", new Vector3(left, top, 0f), 0.5f, new Color(1f, 0.8f, 0.3f));
            m_Body = WorldText.Create(m_Panel.transform, "Texto", new Vector3(left, top - 0.7f, 0f), 0.45f, Color.white);
            m_Choices = WorldText.Create(m_Panel.transform, "Opciones", new Vector3(left, -size.y * 0.5f + 0.3f, 0f), 0.4f,
                new Color(0.6f, 0.9f, 1f), TextAnchor.LowerLeft);
        }
    }
}
