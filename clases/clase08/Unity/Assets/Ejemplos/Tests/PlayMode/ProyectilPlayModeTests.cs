using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Clase08.Tests.PlayMode
{
    public class ProyectilPlayModeTests
    {
        private Scene escena;

        [UnityTest]
        public IEnumerator ColisionarConEnemigo_ReduceSuSalud()
        {
            // Arrange: mundo físico propio, independiente de la escena abierta.
            escena = SceneManager.CreateScene("PruebaProyectil",
                new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            var fisica = escena.GetPhysicsScene();

            var enemigo = new GameObject("Enemigo");
            SceneManager.MoveGameObjectToScene(enemigo, escena);
            enemigo.AddComponent<SphereCollider>().radius = 0.5f;
            var salud = enemigo.AddComponent<Salud>();

            var bala = new GameObject("Proyectil");
            SceneManager.MoveGameObjectToScene(bala, escena);
            bala.transform.position = new Vector3(0.9f, 0f, 0f);
            bala.AddComponent<SphereCollider>().radius = 0.5f;
            var cuerpo = bala.AddComponent<Rigidbody>();
            cuerpo.useGravity = false;
            var proyectil = bala.AddComponent<Proyectil>();
            proyectil.Danio = 25;

            // Act: contacto real entre esferas ligeramente superpuestas.
            // Las escenas físicas locales se simulan explícitamente.
            for (int paso = 0; paso < 10 && salud.Puntos == 100; paso++)
            {
                fisica.Simulate(0.02f);
                yield return null;
            }

            // Assert: nunca llamamos a Impactar desde este test.
            Assert.That(salud.Puntos, Is.EqualTo(75));
        }

        [UnityTearDown]
        public IEnumerator Limpiar()
        {
            // Se ejecuta también cuando falla un Assert.
            if (escena.IsValid() && escena.isLoaded)
                yield return SceneManager.UnloadSceneAsync(escena);
        }
    }
}
