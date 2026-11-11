using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clase14.Tests.PlayMode
{
    /// <summary>
    /// Conversa con un DialogueNpc como lo haría el jugador, pero llamando a los métodos que usa el
    /// Update en vez de simular teclas. El diálogo tiene la misma forma que Guardia.dialogue.
    /// </summary>
    public class DialogueNpcTests
    {
        GameObject m_Root;
        DialogueAsset m_Dialogue;

        static DialogueAsset.Line Line(string text, int next = DialogueAsset.End) =>
            new DialogueAsset.Line { speaker = "Guardia", text = text, next = next };

        [SetUp]
        public void SetUp()
        {
            // 0 saludo -> 1 pregunta -> (2 sí | 3 no) -> fin
            m_Dialogue = ScriptableObject.CreateInstance<DialogueAsset>();
            m_Dialogue.start = 0;
            m_Dialogue.lines.Add(Line("¡Alto!", next: 1));
            var ask = Line("¿Venís a defender la base?");
            ask.choices.Add(new DialogueAsset.Choice { text = "Sí", next = 2 });
            ask.choices.Add(new DialogueAsset.Choice { text = "No", next = 3 });
            m_Dialogue.lines.Add(ask);
            m_Dialogue.lines.Add(Line("Bienvenido."));
            m_Dialogue.lines.Add(Line("Entonces apurate."));
            m_Root = new GameObject("Prueba");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(m_Root);
            Object.Destroy(m_Dialogue);
        }

        DialogueNpc CreateNpc(out DialogueBox box, out PlayerMover player)
        {
            box = new GameObject("Caja").AddComponent<DialogueBox>();
            box.transform.SetParent(m_Root.transform);
            player = new GameObject("Jugador").AddComponent<PlayerMover>();
            player.transform.SetParent(m_Root.transform);
            var npc = new GameObject("Guardia").AddComponent<DialogueNpc>();
            npc.transform.SetParent(m_Root.transform);
            npc.dialogue = m_Dialogue;
            npc.box = box;
            npc.player = player.transform;
            return npc;
        }

        [UnityTest]
        public IEnumerator Hablar_EligiendoLaSegundaOpcion_MuestraEsaRamaYTermina()
        {
            var npc = CreateNpc(out var box, out var player);
            yield return null;
            Assert.That(box.IsVisible, Is.False, "La caja arranca oculta");

            npc.StartTalking();
            Assert.That(box.IsVisible, Is.True);
            Assert.That(box.Speaker, Is.EqualTo("Guardia"));
            Assert.That(box.Body, Is.EqualTo("¡Alto!"));
            Assert.That(player.enabled, Is.False, "El jugador no camina mientras habla");

            npc.Continue();
            Assert.That(box.Body, Is.EqualTo("¿Venís a defender la base?"));
            Assert.That(box.Choices, Does.Contain("[1] Sí").And.Contain("[2] No"));

            npc.Choose(1);
            Assert.That(box.Body, Is.EqualTo("Entonces apurate."));

            npc.Continue();
            yield return null;
            Assert.That(npc.IsTalking, Is.False);
            Assert.That(box.IsVisible, Is.False);
            Assert.That(player.enabled, Is.True);
        }
    }
}
