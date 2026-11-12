using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;

namespace Clase14.Editor.TurretCycle
{
    public readonly struct TurretCycleIssue
    {
        public enum Level { Warning, Error }

        public readonly Level Severity;
        public readonly string Message;
        public readonly INode Node;

        public TurretCycleIssue(Level severity, string message, INode node = null)
        {
            Severity = severity;
            Message = message;
            Node = node;
        }

        public override string ToString() => $"{Severity}: {Message}";
    }

    /// <summary>
    /// Reglas del ciclo de torreta. Es una función pura sobre el grafo: la usan OnGraphChanged (para
    /// las marcas en la ventana) y los tests.
    /// </summary>
    public static class TurretCycleValidation
    {
        public static List<TurretCycleIssue> FindIssues(Graph graph)
        {
            var issues = new List<TurretCycleIssue>();
            var nodes = graph.GetNodes().ToList();
            var starts = nodes.OfType<CycleStartNode>().ToList();

            if (starts.Count == 0)
            {
                issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Error, "Falta un nodo Inicio."));
                return issues;
            }
            foreach (var extra in starts.Skip(1))
                issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Warning, "Hay más de un Inicio: solo se usa el primero.", extra));

            var start = starts[0];
            if (Next(start, TurretCyclePorts.Next) == null)
                issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Error, "El Inicio no está conectado a ningún Estado.", start));

            foreach (var state in nodes.OfType<StateNode>())
            {
                var canFire = Read(state, TurretCyclePorts.CanFire, true);
                for (var i = 0; i < state.TransitionCount; i++)
                {
                    if (Next(state, TurretCyclePorts.TransitionOut(i)) == null)
                        issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Warning, $"La transición {i + 1} no lleva a ningún Estado.", state));
                    if (!canFire && Read(state, TurretCyclePorts.Condition(i), TurretCondition.TimeElapsed) == TurretCondition.ShotsFired)
                        issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Warning,
                            $"El Estado no puede disparar: la transición {i + 1} (Disparos ≥ N) nunca se cumpliría.", state));
                }
            }

            var reachable = Reachable(start);
            foreach (var state in nodes.OfType<StateNode>())
                if (!reachable.Contains(state))
                    issues.Add(new TurretCycleIssue(TurretCycleIssue.Level.Warning, "Estado inalcanzable desde el Inicio.", state));

            return issues;
        }

        /// <summary>El nodo al que lleva una salida de flujo, o null si está desconectada.</summary>
        public static INode Next(INode node, string outputPort) =>
            node.GetOutputPortByName(outputPort)?.FirstConnectedPort?.GetNode();

        /// <summary>
        /// El valor de una entrada: el escrito en el nodo o, si hay un cable, el de la constante o
        /// variable conectada. Si no se puede leer, <paramref name="fallback"/>.
        /// </summary>
        public static T Read<T>(INode node, string inputPort, T fallback = default)
        {
            var port = node.GetInputPortByName(inputPort);
            if (port == null) return fallback;
            if (port.IsConnected)
            {
                var source = port.FirstConnectedPort.GetNode();
                if (source is IConstantNode constant && constant.TryGetValue<T>(out var constantValue)) return constantValue;
                if (source is IVariableNode variable && variable.Variable.TryGetDefaultValue<T>(out var variableValue)) return variableValue;
            }
            return port.TryGetValue<T>(out var value) ? value : fallback;
        }

        /// <summary>
        /// Todos los nodos a los que se llega siguiendo salidas desde <paramref name="start"/>. Como el
        /// grafo tiene ciclos, se marca cada nodo visto para no recorrerlo dos veces.
        /// </summary>
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
