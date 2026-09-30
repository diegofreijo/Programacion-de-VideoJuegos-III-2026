using NUnit.Framework;
using UnityEngine;

namespace Clase08.Tests.EditMode
{
    public class PuertaEditModeTests
    {
        [Test]
        public void Puerta_SoloSeAbreConLaLlaveCorrespondiente()
        {
            var objeto = new GameObject("Puerta");
            try
            {
                // Arrange.
                var puerta = objeto.AddComponent<Puerta>();
                var inventario = new Inventario();

                // Act + Assert: tres pasos de un escenario funcional.
                Assert.That(puerta.IntentarAbrir(inventario), Is.False);
                Assert.That(puerta.Abierta, Is.False);

                inventario.RecogerLlave("llave-azul");
                Assert.That(puerta.IntentarAbrir(inventario), Is.False);
                Assert.That(puerta.Abierta, Is.False);

                inventario.RecogerLlave("llave-roja");
                Assert.That(puerta.IntentarAbrir(inventario), Is.True);
                Assert.That(puerta.Abierta, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(objeto);
            }
        }
    }
}
