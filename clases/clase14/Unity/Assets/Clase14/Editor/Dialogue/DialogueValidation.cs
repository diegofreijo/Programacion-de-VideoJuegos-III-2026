using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;

namespace Clase14.Editor.Dialogue
{
    public readonly struct DialogueIssue
    {
        public enum Level { Info, Warning, Error }

        public readonly Level Severity;
        public readonly string Message;
        public readonly INode Node;

        public DialogueIssue(Level severity, string message, INode node = null)
        {
            Severity = severity;
            Message = message;
            Node = node;
        }

        public override string ToString() => $"{Severity}: {Message}";
    }

    /// <summary>
    /// Reglas del grafo de diálogo. Es una función pura sobre el grafo: la usan OnGraphChanged
    /// (para las marcas en la ventana) y los tests.
    /// </summary>
    public static class DialogueValidation
    {
        public static List<DialogueIssue> FindIssues(Graph graph)
        {
            var issues = new List<DialogueIssue>();
            var nodes = graph.GetNodes().ToList();
            var starts = nodes.OfType<StartNode>().ToList();

            if (starts.Count == 0)
            {
                issues.Add(new DialogueIssue(DialogueIssue.Level.Error, "Falta un nodo Inicio."));
                return issues;
            }
            foreach (var extra in starts.Skip(1))
                issues.Add(new DialogueIssue(DialogueIssue.Level.Warning, "Hay más de un Inicio: solo se usa el primero.", extra));

            var start = starts[0];
            if (Next(start, DialoguePorts.Next) == null)
                issues.Add(new DialogueIssue(DialogueIssue.Level.Error, "El Inicio no está conectado a ninguna línea.", start));

            foreach (var node in nodes)
            {
                if (node is LineNode || node is ChoiceNode)
                {
                    if (string.IsNullOrWhiteSpace(ReadString(node, DialoguePorts.Text)))
                        issues.Add(new DialogueIssue(DialogueIssue.Level.Warning, "La línea no tiene texto.", node));
                }
                if (node is ChoiceNode choice)
                {
                    for (var i = 0; i < choice.ChoiceCount; i++)
                    {
                        if (string.IsNullOrWhiteSpace(ReadString(node, DialoguePorts.ChoiceText(i))))
                            issues.Add(new DialogueIssue(DialogueIssue.Level.Warning, $"La opción {i + 1} no tiene texto.", node));
                        if (Next(node, DialoguePorts.ChoiceOut(i)) == null)
                            issues.Add(new DialogueIssue(DialogueIssue.Level.Warning, $"La opción {i + 1} no lleva a ningún nodo.", node));
                    }
                }
                if (node is LineNode && Next(node, DialoguePorts.Next) == null)
                    issues.Add(new DialogueIssue(DialogueIssue.Level.Info, "La línea termina el diálogo: conviene un nodo Fin.", node));
            }

            var reachable = Reachable(start);
            foreach (var node in nodes)
                if (!(node is StartNode) && IsDialogueNode(node) && !reachable.Contains(node))
                    issues.Add(new DialogueIssue(DialogueIssue.Level.Warning, "Nodo inalcanzable desde el Inicio.", node));

            return issues;
        }

        static bool IsDialogueNode(INode node) => node is LineNode || node is ChoiceNode || node is EndNode;

        /// <summary>El nodo al que lleva una salida de flujo, o null si está desconectada.</summary>
        public static INode Next(INode node, string outputPort) =>
            node.GetOutputPortByName(outputPort)?.FirstConnectedPort?.GetNode();

        /// <summary>
        /// El texto de una entrada: el escrito en el nodo o, si hay un cable, el de la constante o
        /// variable conectada.
        /// </summary>
        public static string ReadString(INode node, string inputPort)
        {
            var port = node.GetInputPortByName(inputPort);
            if (port == null) return null;
            if (port.IsConnected)
            {
                var source = port.FirstConnectedPort.GetNode();
                if (source is IConstantNode constant && constant.TryGetValue<string>(out var constantValue)) return constantValue;
                if (source is IVariableNode variable && variable.Variable.TryGetDefaultValue<string>(out var variableValue)) return variableValue;
            }
            port.TryGetValue<string>(out var value);
            return value;
        }

        /// <summary>Todos los nodos a los que se llega siguiendo salidas desde <paramref name="start"/>.</summary>
        public static HashSet<INode> Reachable(INode start)
        {
            var seen = new HashSet<INode> { start };
            var pending = new Queue<INode>();
            pending.Enqueue(start);
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                foreach (var output in node.GetOutputPorts())
                {
                    var next = output.FirstConnectedPort?.GetNode();
                    if (next != null && seen.Add(next))
                        pending.Enqueue(next);
                }
            }
            return seen;
        }
    }
}
