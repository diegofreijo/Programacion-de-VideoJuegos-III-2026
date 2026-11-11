using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Un personaje con el que se puede hablar. Usa el DialogueAsset que generó el importer a partir
    /// del grafo: recorre las líneas con un DialogueCursor y las muestra en la DialogueBox.
    /// </summary>
    public class DialogueNpc : MonoBehaviour
    {
        public DialogueAsset dialogue;
        public DialogueBox box;
        public Transform player;
        [Tooltip("Distancia desde la que el jugador puede hablarle.")]
        [Min(0.5f)] public float talkRadius = 2.2f;
        public Color color = new Color(0.9f, 0.6f, 0.2f);

        DialogueCursor m_Cursor;
        TextMesh m_Prompt;

        public bool IsTalking => m_Cursor != null;
        public DialogueAsset.Line CurrentLine => m_Cursor?.Current;
        bool PlayerInRange => player != null && Vector2.Distance(player.position, transform.position) <= talkRadius;

        void Awake()
        {
            if (GetComponent<SpriteRenderer>() == null)
                SpriteFactory.AddRenderer(gameObject, SpriteFactory.Square, color, 2);
            WorldText.Create(transform, "Nombre", new Vector3(0f, -0.7f, 0f), 0.35f, Color.white, TextAnchor.UpperCenter).text = name;
            m_Prompt = WorldText.Create(transform, "Aviso", new Vector3(0f, 0.8f, 0f), 0.4f, new Color(1f, 0.9f, 0.4f), TextAnchor.LowerCenter);
            m_Prompt.text = "[E] Hablar";
            m_Prompt.gameObject.SetActive(false);
        }

        // La entrada se lee acá y llama a los mismos métodos públicos que usa el test de Play Mode.
        void Update()
        {
            if (!IsTalking)
            {
                m_Prompt.gameObject.SetActive(PlayerInRange);
                if (PlayerInRange && Input.GetKeyDown(KeyCode.E))
                    StartTalking();
                return;
            }

            var line = CurrentLine;
            if (line.choices.Count > 0)
            {
                for (var i = 0; i < line.choices.Count && i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                        Choose(i);
            }
            else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                Continue();
            }
        }

        public void StartTalking()
        {
            if (dialogue == null || IsTalking) return;
            m_Cursor = new DialogueCursor(dialogue);
            m_Prompt.gameObject.SetActive(false);
            SetPlayerCanMove(false);
            Refresh();
        }

        /// <summary>Pasa a la línea siguiente (en una línea sin opciones).</summary>
        public void Continue() => Advance(0);

        /// <summary>Elige la opción <paramref name="index"/> (desde 0) de la línea actual.</summary>
        public void Choose(int index) => Advance(index);

        void Advance(int choice)
        {
            if (!IsTalking) return;
            m_Cursor.Advance(choice);
            Refresh();
        }

        void Refresh()
        {
            if (m_Cursor.IsFinished)
            {
                m_Cursor = null;
                if (box != null) box.Hide();
                SetPlayerCanMove(true);
                return;
            }
            if (box != null) box.Show(m_Cursor.Current);
        }

        void SetPlayerCanMove(bool canMove)
        {
            if (player != null && player.TryGetComponent<PlayerMover>(out var mover))
                mover.enabled = canMove;
        }
    }
}
