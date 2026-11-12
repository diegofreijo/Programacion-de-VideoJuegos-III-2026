using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Clase14.Editor.TurretCycle
{
    /// <summary>
    /// Ciclo de estados de una torreta, editable con Graph Toolkit. Cada archivo .turretcycle es uno de
    /// estos grafos. A diferencia del diálogo, este grafo tiene un ciclo (Activa → Recalentada →
    /// Enfriando → Activa) y caminos que convergen: no se puede escribir como una lista.
    /// </summary>
    [Graph(AssetExtension)]
    [Serializable]
    public class TurretCycleGraph : Graph
    {
        public const string AssetExtension = "turretcycle";

        [MenuItem("Assets/Create/Clase14/Ciclo de torreta", priority = 3)]
        static void CreateAssetFile() => GraphDatabase.PromptInProjectBrowserToCreateNewAsset<TurretCycleGraph>("NuevoCiclo");

        // Los mensajes aparecen como marcas sobre los nodos y en la consola. Acá solo se avisa.
        public override void OnGraphChanged(GraphLogger logger)
        {
            foreach (var issue in TurretCycleValidation.FindIssues(this))
            {
                if (issue.Severity == TurretCycleIssue.Level.Error)
                    logger.LogError(issue.Message, issue.Node);
                else
                    logger.LogWarning(issue.Message, issue.Node);
            }
        }
    }
}
