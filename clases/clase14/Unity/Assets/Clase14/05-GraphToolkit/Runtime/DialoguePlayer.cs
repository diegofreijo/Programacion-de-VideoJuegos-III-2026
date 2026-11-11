using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Muestra que el juego consume el asset que genera el importer, no el grafo: recorre el
    /// diálogo eligiendo siempre la misma opción y lo escribe en la consola.
    /// </summary>
    public class DialoguePlayer : MonoBehaviour
    {
        public DialogueAsset dialogue;
        [Tooltip("Índice de la opción que se elige en cada pregunta.")]
        [Min(0)] public int alwaysChoose;

        void Start()
        {
            if (dialogue == null) return;
            var cursor = new DialogueCursor(dialogue);
            var guard = 0;
            while (!cursor.IsFinished && guard++ < 100)
            {
                var line = cursor.Current;
                Debug.Log($"[Diálogo] {line.speaker}: {line.text}");
                for (var i = 0; i < line.choices.Count; i++)
                    Debug.Log($"[Diálogo]   {i + 1}. {line.choices[i].text}");
                cursor.Advance(alwaysChoose);
            }
        }
    }
}
