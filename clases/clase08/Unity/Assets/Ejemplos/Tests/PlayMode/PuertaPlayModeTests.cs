using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clase08.Tests.PlayMode
{
    public class PuertaPlayModeTests
    {
        private GameObject objeto;

        [Test]
        public void RecogerLlaveRoja_PermiteAbrirLaPuerta()
        {
            // Arrange.
            objeto = new GameObject("Puerta");
            var puerta = objeto.AddComponent<Puerta>();
            var inventario = new Inventario();
            Assert.That(puerta.IntentarAbrir(inventario), Is.False);
            Assert.That(puerta.Abierta, Is.False);

            // Act: representamos acciones del jugador mediante métodos.
            inventario.RecogerLlave("llave-roja");
            bool pudoAbrir = puerta.IntentarAbrir(inventario);

            // Assert.
            Assert.That(pudoAbrir, Is.True);
            Assert.That(puerta.Abierta, Is.True);
        }

        [UnityTearDown]
        public IEnumerator Limpiar()
        {
            if (objeto != null)
                Object.Destroy(objeto);
            yield return null;
        }
    }
}
