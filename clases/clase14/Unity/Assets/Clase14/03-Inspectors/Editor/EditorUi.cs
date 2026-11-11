using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clase14.Editor
{
    /// <summary>Piezas de UI Toolkit compartidas por los Inspectors y la ventana de balanceo.</summary>
    public static class EditorUi
    {
        public static readonly Color Ok = new Color(0.35f, 0.75f, 0.4f);
        public static readonly Color Bad = new Color(0.9f, 0.3f, 0.3f);

        /// <summary>Torreta de referencia con la que se estiman los tiempos de las vistas previas.</summary>
        public const float ReferenceTurretDamage = 4f;
        public const float ReferenceTurretShotsPerSecond = 3f;

        public static VisualElement Section(string title)
        {
            var box = new VisualElement
            {
                style =
                {
                    marginTop = 10, paddingTop = 6, paddingBottom = 6, paddingLeft = 8, paddingRight = 8,
                    borderTopWidth = 1, borderBottomWidth = 1, borderLeftWidth = 1, borderRightWidth = 1,
                    borderTopColor = Gray, borderBottomColor = Gray, borderLeftColor = Gray, borderRightColor = Gray,
                    borderTopLeftRadius = 4, borderTopRightRadius = 4, borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 0.08f),
                },
            };
            box.Add(new Label(title) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4 } });
            return box;
        }

        static readonly Color Gray = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        public static Label Stat() => new Label { style = { marginLeft = 2, whiteSpace = WhiteSpace.Normal } };

        /// <summary>Vuelve a llenar <paramref name="container"/> con un HelpBox por problema, o uno verde si no hay.</summary>
        public static void ShowProblems(VisualElement container, List<string> problems)
        {
            container.Clear();
            if (problems.Count == 0)
            {
                container.Add(new HelpBox("Sin problemas de diseño.", HelpBoxMessageType.Info));
                return;
            }
            foreach (var problem in problems)
                container.Add(new HelpBox(problem, HelpBoxMessageType.Error));
        }
    }
}
