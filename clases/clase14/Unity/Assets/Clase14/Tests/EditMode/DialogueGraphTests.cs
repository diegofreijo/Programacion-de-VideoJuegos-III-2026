using System.Linq;
using Clase14.Editor;
using Clase14.Editor.Dialogue;
using NUnit.Framework;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Clase14.Tests.EditMode
{
    /// <summary>
    /// Arma grafos de diálogo por código, los valida y los compila al asset de runtime, igual que
    /// el importer cuando se guarda un .dialogue.
    /// </summary>
    public class DialogueGraphTests
    {
        const string Folder = "Assets/Clase14TestsTemp";
        const string Path = Folder + "/Prueba." + DialogueGraph.AssetExtension;

        [SetUp]
        public void SetUp()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "Clase14TestsTemp");
        }

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(Folder);

        static DialogueGraph SampleGraph()
        {
            var graph = GraphDatabase.CreateGraph<DialogueGraph>(Path);
            SampleContent.BuildSampleGraph(graph);
            GraphDatabase.SaveGraph(graph);
            return graph;
        }

        [Test]
        public void GrafoDeEjemplo_NoTieneErroresNiAdvertencias()
        {
            var issues = DialogueValidation.FindIssues(SampleGraph());
            Assert.That(issues.Where(i => i.Severity != DialogueIssue.Level.Info), Is.Empty);
        }

        [Test]
        public void GrafoVacio_ReportaQueFaltaElInicio()
        {
            var graph = GraphDatabase.CreateGraph<DialogueGraph>(Path);
            var issues = DialogueValidation.FindIssues(graph);
            Assert.That(issues.Single().Severity, Is.EqualTo(DialogueIssue.Level.Error));
            Assert.That(issues.Single().Message, Does.Contain("Inicio"));
        }

        [Test]
        public void NodoSuelto_SeMarcaComoInalcanzable()
        {
            var graph = SampleGraph();
            graph.UndoBeginRecordGraph("Nodo suelto");
            graph.AddNode(new LineNode());
            graph.UndoEndRecordGraph();
            var issues = DialogueValidation.FindIssues(graph);
            Assert.That(issues.Select(i => i.Message), Has.Some.Contains("inalcanzable"));
        }

        [Test]
        public void Compilar_ProduceLineasEnlazadas()
        {
            var runtime = DialogueImporter.Compile(SampleGraph());

            Assert.That(runtime.lines.Count, Is.EqualTo(4));
            var cursor = new DialogueCursor(runtime);
            Assert.That(cursor.Current.text, Is.EqualTo("¡Alto! ¿Quién anda ahí?"));
            cursor.Advance();
            Assert.That(cursor.Current.choices.Select(c => c.text),
                Is.EqualTo(new[] { "Sí, traigo la torreta.", "Solo estoy de paso." }));
            cursor.Advance(1);
            Assert.That(cursor.Current.text, Does.Contain("orcos"));
            cursor.Advance();
            Assert.That(cursor.IsFinished, Is.True);
        }

        [Test]
        public void Importer_ElAssetPrincipalEsElDialogoDeRuntime()
        {
            SampleGraph();
            AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceUpdate);
            var runtime = AssetDatabase.LoadMainAssetAtPath(Path) as DialogueAsset;
            Assert.That(runtime, Is.Not.Null);
            Assert.That(runtime.lines.Count, Is.EqualTo(4));
        }
    }
}
