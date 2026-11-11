using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Versión de runtime de un diálogo. La genera el importer a partir del grafo (.dialogue):
    /// el juego nunca ve el grafo, solo esta lista de líneas.
    /// </summary>
    public class DialogueAsset : ScriptableObject
    {
        public const int End = -1;

        [Serializable]
        public class Choice
        {
            public string text;
            public int next = End;
        }

        [Serializable]
        public class Line
        {
            public string speaker;
            [TextArea] public string text;
            [Tooltip("Índice de la próxima línea si no hay opciones; -1 termina el diálogo.")]
            public int next = End;
            public List<Choice> choices = new List<Choice>();
        }

        public int start = End;
        public List<Line> lines = new List<Line>();
    }

    /// <summary>Recorre un DialogueAsset. Sin UI: la consola alcanza para la demo y para los tests.</summary>
    public class DialogueCursor
    {
        readonly DialogueAsset m_Asset;
        public int Index { get; private set; }

        public DialogueCursor(DialogueAsset asset)
        {
            m_Asset = asset;
            Index = asset.start;
        }

        public bool IsFinished => Index == DialogueAsset.End;
        public DialogueAsset.Line Current => IsFinished ? null : m_Asset.lines[Index];

        /// <summary>Avanza. Si la línea tiene opciones, <paramref name="choice"/> elige cuál.</summary>
        public void Advance(int choice = 0)
        {
            if (IsFinished) return;
            var line = Current;
            Index = line.choices.Count > 0 ? line.choices[Mathf.Clamp(choice, 0, line.choices.Count - 1)].next : line.next;
        }
    }
}
