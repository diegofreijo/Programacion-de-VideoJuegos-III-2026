using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clase14.Editor
{
    /// <summary>
    /// Inspector propio de WaveDefinition: muestra la composición de la oleada como una barra de
    /// colores, estima su duración y su dificultad, y marca los problemas.
    /// </summary>
    [CustomEditor(typeof(WaveDefinition))]
    public class WaveDefinitionEditor : UnityEditor.Editor
    {
        VisualElement m_Composition;
        Label m_Count;
        Label m_Duration;
        Label m_Health;
        Label m_ClearTime;
        VisualElement m_Problems;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var preview = EditorUi.Section("Vista previa de la oleada");
            m_Composition = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, height = 18, marginBottom = 6, marginTop = 2 },
                tooltip = "Composición: cada color es un tipo de enemigo, el ancho es la cantidad.",
            };
            preview.Add(m_Composition);
            preview.Add(m_Count = EditorUi.Stat());
            preview.Add(m_Duration = EditorUi.Stat());
            preview.Add(m_Health = EditorUi.Stat());
            preview.Add(m_ClearTime = EditorUi.Stat());
            root.Add(preview);

            var validation = EditorUi.Section("Validaciones");
            validation.Add(m_Problems = new VisualElement());
            root.Add(validation);

            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 10 } };
            buttons.Add(new Button(SortByHealth) { text = "Ordenar de débil a fuerte", style = { flexGrow = 1, height = 26 } });
            buttons.Add(new Button(() => BalanceWindow.Open()) { text = "Abrir ventana de balanceo", style = { flexGrow = 1, height = 26 } });
            root.Add(buttons);

            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            // También los enemigos referenciados: si se editan en la ventana de balanceo, la vista previa se entera.
            foreach (var entry in ((WaveDefinition)target).entries)
                if (entry?.enemy != null)
                    root.TrackSerializedObjectValue(new SerializedObject(entry.enemy), _ => Refresh());
            Refresh();
            return root;
        }

        void Refresh()
        {
            var wave = (WaveDefinition)target;

            m_Composition.Clear();
            foreach (var entry in wave.entries)
            {
                if (entry == null || entry.enemy == null) continue;
                m_Composition.Add(new VisualElement
                {
                    tooltip = $"{entry.count} x {entry.enemy.displayName}",
                    style = { flexGrow = entry.count, backgroundColor = entry.enemy.color, marginRight = 1 },
                });
            }

            m_Count.text = $"Enemigos: {wave.TotalEnemies}";
            m_Duration.text = $"Duración estimada de la aparición: {wave.EstimatedSpawnDuration:0.#} s " +
                              $"(intervalo {wave.spawnInterval} s)";
            m_Health.text = $"Vida total: {wave.TotalHealth:0}. DPS total si llegan todos: {wave.TotalDps:0.#}";
            var turretDps = EditorUi.ReferenceTurretDamage * EditorUi.ReferenceTurretShotsPerSecond;
            m_ClearTime.text = $"La torreta de referencia necesita {wave.TotalHealth / turretDps:0.#} s de disparo para limpiarla.";

            EditorUi.ShowProblems(m_Problems, wave.Validate());
        }

        void SortByHealth()
        {
            var wave = (WaveDefinition)target;
            Undo.RecordObject(wave, "Ordenar oleada");
            wave.entries.Sort((a, b) =>
                (a?.enemy != null ? a.enemy.maxHealth : 0f).CompareTo(b?.enemy != null ? b.enemy.maxHealth : 0f));
            EditorUtility.SetDirty(wave);
            serializedObject.Update();
            Refresh();
        }
    }
}
