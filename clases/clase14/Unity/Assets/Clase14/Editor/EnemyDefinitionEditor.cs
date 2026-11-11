using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clase14.Editor
{
    /// <summary>
    /// Inspector propio de EnemyDefinition en UI Toolkit: los campos de siempre más una vista
    /// previa calculada, las validaciones a la vista y un botón que crea una variante.
    /// </summary>
    [CustomEditor(typeof(EnemyDefinition))]
    public class EnemyDefinitionEditor : UnityEditor.Editor
    {
        Label m_Dps;
        ProgressBar m_DpsBar;
        Label m_TimeToKill;
        VisualElement m_Problems;

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // Los campos por defecto, con sus [Header], [Tooltip], [Range] y [Min].
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var preview = EditorUi.Section("Vista previa");
            m_Dps = EditorUi.Stat();
            m_DpsBar = new ProgressBar { lowValue = 0, highValue = EnemyDefinition.MaxDesignDps };
            m_TimeToKill = EditorUi.Stat();
            preview.Add(m_Dps);
            preview.Add(m_DpsBar);
            preview.Add(m_TimeToKill);
            root.Add(preview);

            var validation = EditorUi.Section("Validaciones");
            m_Problems = new VisualElement();
            validation.Add(m_Problems);
            root.Add(validation);

            root.Add(new Button(CreateEliteVariant)
            {
                text = "Crear variante élite (x2 vida, x1.5 daño)",
                tooltip = "Crea un asset nuevo al lado de este, con los valores aumentados.",
                style = { marginTop = 10, height = 26 },
            });

            // Cada vez que cambia cualquier campo serializado, se recalcula la vista previa.
            root.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            Refresh();
            return root;
        }

        void Refresh()
        {
            var enemy = (EnemyDefinition)target;
            var dps = enemy.EstimatedDps;
            m_Dps.text = $"DPS estimado: {dps:0.##} ({enemy.damage:0.##} de daño x {enemy.attacksPerSecond:0.##} golpes/s)";
            m_DpsBar.value = Mathf.Min(dps, EnemyDefinition.MaxDesignDps);
            m_DpsBar.title = $"{dps:0.#} / {EnemyDefinition.MaxDesignDps} DPS máximo de diseño";

            var seconds = CombatRules.SecondsToKill(enemy, EditorUi.ReferenceTurretDamage, EditorUi.ReferenceTurretShotsPerSecond);
            m_TimeToKill.text = $"La torreta de referencia lo mata en {seconds:0.##} s; " +
                                $"en ese tiempo camina {seconds * enemy.moveSpeed:0.#} unidades.";

            EditorUi.ShowProblems(m_Problems, enemy.Validate());
        }

        void CreateEliteVariant()
        {
            var source = (EnemyDefinition)target;
            var elite = Instantiate(source);
            elite.displayName = $"{source.displayName} élite";
            elite.maxHealth = source.maxHealth * 2f;
            elite.damage = source.damage * 1.5f;
            elite.reward = source.reward * 2;

            var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(source));
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{source.name}Elite.asset");
            AssetDatabase.CreateAsset(elite, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = elite;
            EditorGUIUtility.PingObject(elite);
        }
    }
}
