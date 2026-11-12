using System.Linq;
using Clase14.Editor;
using Clase14.Editor.TurretCycle;
using NUnit.Framework;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine;

namespace Clase14.Tests.EditMode
{
    /// <summary>
    /// Arma ciclos de torreta por código, los valida y los compila al asset de runtime, igual que el
    /// importer cuando se guarda un .turretcycle.
    /// </summary>
    public class TurretCycleGraphTests
    {
        const string Folder = "Assets/Clase14TestsCiclo";
        const string Path = Folder + "/Prueba." + TurretCycleGraph.AssetExtension;

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "Clase14TestsCiclo");
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(Folder);

        static TurretCycleGraph SampleGraph()
        {
            var graph = GraphDatabase.CreateGraph<TurretCycleGraph>(Path);
            SampleContent.BuildSampleTurretCycle(graph);
            GraphDatabase.SaveGraph(graph);
            return graph;
        }

        /// <summary>Un Inicio conectado a un único Estado, con su transición sin conectar.</summary>
        static (TurretCycleGraph graph, StateNode state) OneStateGraph()
        {
            var graph = GraphDatabase.CreateGraph<TurretCycleGraph>(Path);
            graph.UndoBeginRecordGraph("Un Estado");
            var start = new CycleStartNode();
            graph.AddNode(start);
            var state = new StateNode();
            graph.AddNode(state);
            graph.Connect(start.GetOutputPortByName(TurretCyclePorts.Next), state.GetInputPortByName(TurretCyclePorts.In));
            graph.UndoEndRecordGraph();
            return (graph, state);
        }

        static string[] Messages(Graph graph, TurretCycleIssue.Level level) =>
            TurretCycleValidation.FindIssues(graph).Where(i => i.Severity == level).Select(i => i.Message).ToArray();

        [Test]
        public void CicloDeEjemplo_NoTieneErroresNiAvisos()
        {
            Assert.That(TurretCycleValidation.FindIssues(SampleGraph()), Is.Empty);
        }

        [Test]
        public void GrafoVacio_ReportaQueFaltaElInicio()
        {
            var issues = TurretCycleValidation.FindIssues(GraphDatabase.CreateGraph<TurretCycleGraph>(Path));
            Assert.That(issues.Single().Severity, Is.EqualTo(TurretCycleIssue.Level.Error));
            Assert.That(issues.Single().Message, Does.Contain("Inicio"));
        }

        [Test]
        public void InicioSinConectar_EsUnError()
        {
            var graph = GraphDatabase.CreateGraph<TurretCycleGraph>(Path);
            graph.UndoBeginRecordGraph("Inicio suelto");
            graph.AddNode(new CycleStartNode());
            graph.UndoEndRecordGraph();
            Assert.That(Messages(graph, TurretCycleIssue.Level.Error), Has.Some.Contains("no está conectado"));
        }

        [Test]
        public void TransicionSinConectar_EsUnAviso()
        {
            var (graph, _) = OneStateGraph();
            Assert.That(Messages(graph, TurretCycleIssue.Level.Warning), Has.Some.Contains("La transición 1 no lleva"));
        }

        [Test]
        public void EstadoSuelto_SeMarcaComoInalcanzable()
        {
            var graph = SampleGraph();
            graph.UndoBeginRecordGraph("Estado suelto");
            graph.AddNode(new StateNode());
            graph.UndoEndRecordGraph();
            Assert.That(Messages(graph, TurretCycleIssue.Level.Warning), Has.Some.Contains("inalcanzable"));
        }

        [Test]
        public void EstadoQueNoDispara_ConTransicionPorDisparos_EsUnAviso()
        {
            var (graph, state) = OneStateGraph();
            state.GetInputPortByName(TurretCyclePorts.CanFire).TrySetValue(false);
            state.GetInputPortByName(TurretCyclePorts.Condition(0)).TrySetValue(TurretCondition.ShotsFired);
            Assert.That(Messages(graph, TurretCycleIssue.Level.Warning), Has.Some.Contains("nunca se cumpliría"));
        }

        [Test]
        public void Compilar_ProduceLosTresEstadosEnlazados()
        {
            AssertSampleCycle(TurretCycleImporter.Compile(SampleGraph()));
        }

        // Importar desde el disco comprueba además que el enum de la condición sobrevive al guardado.
        [Test]
        public void Importer_ElAssetPrincipalEsElCicloDeRuntime()
        {
            SampleGraph();
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceUpdate);
            AssertSampleCycle(AssetDatabase.LoadMainAssetAtPath(Path) as TurretCycleAsset);
        }

        static void AssertSampleCycle(TurretCycleAsset cycle)
        {
            Assert.That(cycle, Is.Not.Null);
            Assert.That(cycle.states.Select(s => s.name), Is.EqualTo(new[] { "Activa", "Recalentada", "Enfriando" }));
            Assert.That(cycle.start, Is.EqualTo(0));

            var active = cycle.states[0];
            Assert.That(active.canFire, Is.True);
            Assert.That(active.color, Is.EqualTo(new Color(1f, 0.9f, 0.2f)));
            Assert.That(active.transitions.Single().condition, Is.EqualTo(TurretCondition.ShotsFired));
            Assert.That(active.transitions.Single().value, Is.EqualTo(10f));
            Assert.That(active.transitions.Single().next, Is.EqualTo(1));

            var hot = cycle.states[1];
            Assert.That(hot.canFire, Is.False);
            Assert.That(hot.transitions.Single().condition, Is.EqualTo(TurretCondition.TimeElapsed));
            Assert.That(hot.transitions.Single().value, Is.EqualTo(2f));
            Assert.That(hot.transitions.Single().next, Is.EqualTo(2));

            var cooling = cycle.states[2];
            Assert.That(cooling.fireRateMultiplier, Is.EqualTo(0.5f));
            Assert.That(cooling.transitions.Select(t => t.condition),
                Is.EqualTo(new[] { TurretCondition.TimeElapsed, TurretCondition.NoEnemiesInRange }));
            Assert.That(cooling.transitions.Select(t => t.next), Is.EqualTo(new[] { 0, 0 }));
        }
    }
}
