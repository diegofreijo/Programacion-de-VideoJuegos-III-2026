using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Clase14.Editor.TurretCycle
{
    /// <summary>
    /// Convierte cada .turretcycle en un TurretCycleAsset de runtime al importarlo (cada vez que se
    /// guarda el grafo). El juego usa el TurretCycleAsset; el grafo existe solo en el Editor.
    /// </summary>
    // La versión se sube cuando cambia cómo se importa: obliga a Unity a reimportar todos los .turretcycle.
    [ScriptedImporter(1, TurretCycleGraph.AssetExtension)]
    public class TurretCycleImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<TurretCycleGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"No se pudo cargar el ciclo de torreta: {ctx.assetPath}");
                return;
            }

            var runtime = Compile(graph);
            runtime.name = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            ctx.AddObjectToAsset("Runtime", runtime);
            ctx.SetMainObject(runtime);
        }

        /// <summary>Numera los Estados alcanzables desde el Inicio y enlaza sus transiciones por índice.</summary>
        public static TurretCycleAsset Compile(Graph graph)
        {
            var runtime = ScriptableObject.CreateInstance<TurretCycleAsset>();
            var start = graph.GetNodes().OfType<CycleStartNode>().FirstOrDefault();
            if (start == null) return runtime; // OnGraphChanged ya marcó el error.

            // Primero se numeran los Estados en el orden en que se alcanzan, después se enlazan.
            // Los ciclos no son problema: cada nodo recibe su índice una sola vez.
            var order = TurretCycleValidation.ReachableInOrder(start).OfType<StateNode>().ToList();
            var indices = new Dictionary<INode, int>();
            for (var i = 0; i < order.Count; i++) indices[order[i]] = i;

            int IndexOf(INode node) => node != null && indices.TryGetValue(node, out var i) ? i : TurretCycleAsset.None;

            foreach (var node in order)
            {
                var state = new TurretCycleAsset.State
                {
                    name = TurretCycleValidation.Read(node, TurretCyclePorts.Name, "Estado"),
                    canFire = TurretCycleValidation.Read(node, TurretCyclePorts.CanFire, true),
                    fireRateMultiplier = TurretCycleValidation.Read(node, TurretCyclePorts.FireRate, 1f),
                    color = TurretCycleValidation.Read(node, TurretCyclePorts.Color, Color.white),
                };
                for (var i = 0; i < node.TransitionCount; i++)
                    state.transitions.Add(new TurretCycleAsset.Transition
                    {
                        condition = TurretCycleValidation.Read(node, TurretCyclePorts.Condition(i), TurretCondition.TimeElapsed),
                        value = TurretCycleValidation.Read(node, TurretCyclePorts.Value(i), 0f),
                        next = IndexOf(TurretCycleValidation.Next(node, TurretCyclePorts.TransitionOut(i))),
                    });
                runtime.states.Add(state);
            }

            runtime.start = IndexOf(TurretCycleValidation.Next(start, TurretCyclePorts.Next));
            return runtime;
        }
    }
}
