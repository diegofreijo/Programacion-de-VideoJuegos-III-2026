using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Clase14.Editor.Dialogue
{
    /// <summary>
    /// Convierte cada .dialogue en un DialogueAsset de runtime al importarlo (cada vez que se
    /// guarda el grafo). El juego usa el DialogueAsset; el grafo existe solo en el Editor.
    /// </summary>
    [ScriptedImporter(1, DialogueGraph.AssetExtension)]
    public class DialogueImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"No se pudo cargar el grafo de diálogo: {ctx.assetPath}");
                return;
            }

            var runtime = Compile(graph);
            runtime.name = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            ctx.AddObjectToAsset("Runtime", runtime);
            ctx.SetMainObject(runtime);
        }

        /// <summary>Recorre el grafo desde el Inicio y arma la lista de líneas.</summary>
        public static DialogueAsset Compile(Graph graph)
        {
            var runtime = ScriptableObject.CreateInstance<DialogueAsset>();
            var start = graph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (start == null) return runtime; // OnGraphChanged ya marcó el error.

            // Primero se numeran las líneas en el orden en que se alcanzan, después se enlazan.
            var indices = new Dictionary<INode, int>();
            var order = new List<INode>();
            var pending = new Queue<INode>();
            pending.Enqueue(start);
            var seen = new HashSet<INode> { start };
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                if (node is LineNode || node is ChoiceNode)
                {
                    indices[node] = order.Count;
                    order.Add(node);
                }
                foreach (var output in node.GetOutputPorts())
                {
                    var next = output.FirstConnectedPort?.GetNode();
                    if (next != null && seen.Add(next))
                        pending.Enqueue(next);
                }
            }

            int IndexOf(INode node) => node != null && indices.TryGetValue(node, out var i) ? i : DialogueAsset.End;

            foreach (var node in order)
            {
                var line = new DialogueAsset.Line
                {
                    speaker = DialogueValidation.ReadString(node, DialoguePorts.Speaker),
                    text = DialogueValidation.ReadString(node, DialoguePorts.Text),
                };
                if (node is ChoiceNode choice)
                {
                    for (var i = 0; i < choice.ChoiceCount; i++)
                        line.choices.Add(new DialogueAsset.Choice
                        {
                            text = DialogueValidation.ReadString(node, DialoguePorts.ChoiceText(i)),
                            next = IndexOf(DialogueValidation.Next(node, DialoguePorts.ChoiceOut(i))),
                        });
                }
                else
                {
                    line.next = IndexOf(DialogueValidation.Next(node, DialoguePorts.Next));
                }
                runtime.lines.Add(line);
            }

            runtime.start = IndexOf(DialogueValidation.Next(start, DialoguePorts.Next));
            return runtime;
        }
    }
}
