using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine.Scripting.APIUpdating;

namespace Clase14.Editor.Dialogue
{
    /// <summary>
    /// Grafo de diálogo editable con Graph Toolkit. Cada archivo .dialogue es uno de estos grafos;
    /// la ventana de nodos la provee Unity, acá solo se declaran el grafo, los nodos y las reglas.
    /// </summary>
    // MovedFrom: estas clases estaban en el assembly "Clase14.Editor". Los .dialogue guardan el
    // nombre del assembly de cada nodo; sin esto, al renombrarlo los nodos quedan "Missing".
    [MovedFrom(true, sourceAssembly: "Clase14.Editor")]
    [Graph(AssetExtension)]
    [Serializable]
    public class DialogueGraph : Graph
    {
        public const string AssetExtension = "dialogue";

        [MenuItem("Assets/Create/Clase14/Diálogo", priority = 2)]
        static void CreateAssetFile() => GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>("NuevoDialogo");

        // Se llama cada vez que el grafo cambia. Los mensajes aparecen como marcas sobre los nodos
        // y en la consola. Acá no se puede modificar el grafo: solo avisar.
        public override void OnGraphChanged(GraphLogger logger)
        {
            foreach (var issue in DialogueValidation.FindIssues(this))
            {
                switch (issue.Severity)
                {
                    case DialogueIssue.Level.Error:
                        logger.LogError(issue.Message, issue.Node);
                        break;
                    case DialogueIssue.Level.Warning:
                        logger.LogWarning(issue.Message, issue.Node);
                        break;
                    default:
                        logger.Log(issue.Message, issue.Node);
                        break;
                }
            }
        }
    }
}
