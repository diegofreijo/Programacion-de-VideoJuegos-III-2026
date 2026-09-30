using NUnit.Framework;
using UnityEngine;

namespace Clase08.Tests.EditMode
{
    public class ProyectilEditModeTests
    {
        [Test]
        public void Impactar_ReduceLaSaludDelEnemigo()
        {
            var enemigo = new GameObject("Enemigo");
            var bala = new GameObject("Proyectil");
            try
            {
                // Arrange.
                var salud = enemigo.AddComponent<Salud>();
                var proyectil = bala.AddComponent<Proyectil>();
                proyectil.Danio = 25;
                // Act: no estamos comprobando la detección de colisiones.
                proyectil.Impactar(salud);
                // Assert.
                Assert.That(salud.Puntos, Is.EqualTo(75));
            }
            finally
            {
                Object.DestroyImmediate(bala);
                Object.DestroyImmediate(enemigo);
            }
        }
    }
}
