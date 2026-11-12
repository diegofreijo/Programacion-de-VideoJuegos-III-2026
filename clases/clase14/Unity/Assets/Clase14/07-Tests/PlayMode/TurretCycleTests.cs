using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clase14.Tests.PlayMode
{
    /// <summary>El componente TurretCycle con frames reales: aplica el Estado a la torreta.</summary>
    public class TurretCycleTests
    {
        readonly List<Object> m_Created = new List<Object>();

        T Track<T>(T obj) where T : Object
        {
            m_Created.Add(obj);
            return obj;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var enemy in new List<Enemy>(Enemy.Alive))
                Object.Destroy(enemy.gameObject);
            foreach (var obj in m_Created)
                if (obj != null) Object.Destroy(obj);
            m_Created.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator EstadoQueNoDispara_DejaALaTorretaQuietaYRoja()
        {
            var red = new Color(0.9f, 0.15f, 0.1f);
            var cycle = Track(ScriptableObject.CreateInstance<TurretCycleAsset>());
            cycle.start = 0;
            cycle.states.Add(new TurretCycleAsset.State { name = "Recalentada", canFire = false, color = red });

            // Se desactiva mientras se arma, para asignar el ciclo antes de su Awake.
            var go = Track(new GameObject("Torreta"));
            go.SetActive(false);
            var turret = go.AddComponent<Turret>();
            turret.shotsPerSecond = 20f;
            go.AddComponent<TurretCycle>().cycle = cycle;
            go.SetActive(true);

            var definition = Track(ScriptableObject.CreateInstance<EnemyDefinition>());
            definition.maxHealth = 1f;
            definition.moveSpeed = 0f;
            var enemy = new GameObject().AddComponent<Enemy>();
            enemy.transform.position = new Vector3(2f, 0f);
            enemy.Init(definition, Track(new GameObject("Objetivo")).AddComponent<DefendedTarget>());

            yield return new WaitForSeconds(0.3f);

            Assert.That(enemy != null, Is.True, "La torreta no debería haber disparado");
            Assert.That(turret.canFire, Is.False);
            Assert.That(go.GetComponent<SpriteRenderer>().color, Is.EqualTo(red));
        }
    }
}
