using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clase14.Editor
{
    /// <summary>
    /// Ventana de balanceo: todos los EnemyDefinition del proyecto en una tabla editable, con los
    /// datos inválidos marcados en rojo. Editar una celda modifica el asset (con Undo).
    /// </summary>
    public class BalanceWindow : EditorWindow
    {
        public const string EnemiesFolder = "Assets/Clase14/Data/Enemigos";

        readonly List<EnemyDefinition> m_Enemies = new List<EnemyDefinition>();
        MultiColumnListView m_Table;
        Label m_Summary;

        [MenuItem("Clase14/Ventana de balanceo %#b", priority = 0)]
        public static BalanceWindow Open()
        {
            var window = GetWindow<BalanceWindow>();
            window.titleContent = new GUIContent("Balanceo de enemigos");
            window.minSize = new Vector2(720, 260);
            return window;
        }

        [MenuItem("Clase14/Crear enemigo", priority = 20)]
        static void CreateEnemyMenu()
        {
            var enemy = CreateEnemyAsset();
            Selection.activeObject = enemy;
            EditorGUIUtility.PingObject(enemy);
        }

        /// <summary>Crea un EnemyDefinition nuevo en la carpeta de enemigos, con nombre único.</summary>
        public static EnemyDefinition CreateEnemyAsset(string baseName = "NuevoEnemigo")
        {
            if (!AssetDatabase.IsValidFolder(EnemiesFolder))
            {
                Directory.CreateDirectory(EnemiesFolder);
                AssetDatabase.Refresh();
            }
            var path = AssetDatabase.GenerateUniqueAssetPath($"{EnemiesFolder}/{baseName}.asset");
            var enemy = CreateInstance<EnemyDefinition>();
            enemy.displayName = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(enemy, path);
            AssetDatabase.SaveAssets();
            return enemy;
        }

        public static List<EnemyDefinition> FindAllEnemies() =>
            AssetDatabase.FindAssets($"t:{nameof(EnemyDefinition)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(enemy => enemy != null)
                .OrderBy(enemy => enemy.name)
                .ToList();

        public void CreateGUI()
        {
            var toolbar = new Toolbar();
            toolbar.Add(new ToolbarButton(() =>
            {
                CreateEnemyAsset();
                Reload();
            }) { text = "+ Nuevo enemigo" });
            toolbar.Add(new ToolbarButton(Reload) { text = "Refrescar" });
            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });
            toolbar.Add(m_Summary = new Label { style = { unityTextAlign = TextAnchor.MiddleRight, marginRight = 6 } });
            rootVisualElement.Add(toolbar);

            m_Table = new MultiColumnListView
            {
                itemsSource = m_Enemies,
                fixedItemHeight = 24,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                selectionType = SelectionType.Single,
                style = { flexGrow = 1 },
            };
            AddBoundColumn<TextField>("Nombre", nameof(EnemyDefinition.displayName), 150);
            AddBoundColumn<ColorField>("Color", nameof(EnemyDefinition.color), 70);
            AddBoundColumn<FloatField>("Vida", nameof(EnemyDefinition.maxHealth), 60);
            AddBoundColumn<Slider>("Velocidad", nameof(EnemyDefinition.moveSpeed), 110, field =>
            {
                field.lowValue = 0.1f;
                field.highValue = 10f;
                field.showInputField = true;
            });
            AddBoundColumn<FloatField>("Daño", nameof(EnemyDefinition.damage), 60);
            AddBoundColumn<FloatField>("Golpes/s", nameof(EnemyDefinition.attacksPerSecond), 70);
            AddComputedColumn("DPS", 55, enemy => enemy.EstimatedDps.ToString("0.#"),
                enemy => enemy.EstimatedDps > EnemyDefinition.MaxDesignDps);
            AddComputedColumn("Estado", 340, enemy =>
            {
                var problems = enemy.Validate();
                return problems.Count == 0 ? "OK" : $"⚠ {problems[0]}" + (problems.Count > 1 ? $" (+{problems.Count - 1})" : "");
            }, enemy => !enemy.IsValid);

            m_Table.selectionChanged += selection =>
            {
                var enemy = selection.OfType<EnemyDefinition>().FirstOrDefault();
                if (enemy != null) Selection.activeObject = enemy;
            };
            rootVisualElement.Add(m_Table);

            Reload();
        }

        // Si se crean o borran assets por fuera de la ventana, la tabla se entera.
        void OnProjectChange() => Reload();

        public void Reload()
        {
            if (m_Table == null) return;
            m_Enemies.Clear();
            m_Enemies.AddRange(FindAllEnemies());
            m_Table.RefreshItems();
            UpdateSummary();
        }

        void UpdateSummary()
        {
            var invalid = m_Enemies.Count(enemy => !enemy.IsValid);
            m_Summary.text = $"{m_Enemies.Count} enemigos, {invalid} con problemas";
            m_Summary.style.color = invalid > 0 ? EditorUi.Bad : EditorUi.Ok;
        }

        /// <summary>Columna con un campo editable ligado a una propiedad serializada del asset.</summary>
        void AddBoundColumn<TField>(string title, string propertyName, float width, Action<TField> setup = null)
            where TField : VisualElement, IBindable, new()
        {
            m_Table.columns.Add(new Column
            {
                title = title,
                width = width,
                stretchable = false,
                makeCell = () =>
                {
                    var field = new TField();
                    setup?.Invoke(field);
                    return field;
                },
                bindCell = (element, index) =>
                {
                    var serialized = new SerializedObject(m_Enemies[index]);
                    ((IBindable)element).BindProperty(serialized.FindProperty(propertyName));
                    // Al cambiar cualquier valor, las columnas calculadas y el resumen se actualizan.
                    element.TrackSerializedObjectValue(serialized, _ =>
                    {
                        m_Table.RefreshItem(index);
                        UpdateSummary();
                    });
                },
                unbindCell = (element, _) => element.Unbind(),
            });
        }

        /// <summary>Columna de solo lectura calculada a partir del asset; <paramref name="isBad"/> la pinta de rojo.</summary>
        void AddComputedColumn(string title, float width, Func<EnemyDefinition, string> text, Func<EnemyDefinition, bool> isBad)
        {
            m_Table.columns.Add(new Column
            {
                title = title,
                width = width,
                // Solo la columna ancha (Estado) se estira con la ventana.
                stretchable = width > 100,
                makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 4 } },
                bindCell = (element, index) =>
                {
                    var enemy = m_Enemies[index];
                    var label = (Label)element;
                    label.text = text(enemy);
                    label.tooltip = string.Join("\n", enemy.Validate());
                    var bad = isBad(enemy);
                    label.style.color = bad ? EditorUi.Bad : EditorUi.Ok;
                    label.style.backgroundColor = bad ? new Color(EditorUi.Bad.r, EditorUi.Bad.g, EditorUi.Bad.b, 0.12f) : Color.clear;
                },
            });
        }
    }
}
