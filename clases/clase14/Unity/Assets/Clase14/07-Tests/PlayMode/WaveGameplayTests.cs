using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clase14.Tests.PlayMode
{
    /// <summary>
    /// Pruebas en Play Mode: crean la escena mínima por código (objetivo, torreta, oleadas) y dejan
    /// correr frames reales.
    /// </summary>
    public class WaveGameplayTests
    {
        readonly List<Object> m_Created = new List<Object>();

        T Track<T>(T obj) where T : Object
        {
            m_Created.Add(obj);
            return obj;
        }

        EnemyDefinition Enemy(float health = 10f, float speed = 2f, float damage = 1f, float rate = 1f)
        {
            var enemy = Track(ScriptableObject.CreateInstance<EnemyDefinition>());
            enemy.displayName = "Prueba";
            enemy.maxHealth = health;
            enemy.moveSpeed = speed;
            enemy.damage = damage;
            enemy.attacksPerSecond = rate;
            return enemy;
        }

        DefendedTarget Target() => Track(new GameObject("Objetivo")).AddComponent<DefendedTarget>();

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var enemy in new List<Enemy>(Clase14.Enemy.Alive))
                Object.Destroy(enemy.gameObject);
            foreach (var obj in m_Created)
                if (obj != null) Object.Destroy(obj);
            m_Created.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Spawner_HaceAparecerTodaLaOleada()
        {
            var wave = Track(ScriptableObject.CreateInstance<WaveDefinition>());
            wave.entries.Add(new WaveEntry { enemy = Enemy(), count = 3 });
            wave.entries.Add(new WaveEntry { enemy = Enemy(), count = 2 });
            wave.spawnInterval = new FloatRange(0f, 0f);

            var spawner = Track(new GameObject("Oleadas")).AddComponent<WaveSpawner>();
            spawner.playOnStart = false;
            spawner.target = Target();

            yield return spawner.PlayWave(wave);

            Assert.That(spawner.SpawnedCount, Is.EqualTo(5));
            Assert.That(Clase14.Enemy.Alive.Count, Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator Enemigo_CaminaHastaElObjetivoYLoGolpea()
        {
            var target = Target();
            var enemy = new GameObject().AddComponent<Enemy>();
            enemy.transform.position = new Vector3(2f, 0f);
            enemy.Init(Enemy(speed: 10f, damage: 5f, rate: 5f), target);

            yield return new WaitForSeconds(0.6f);

            Assert.That(target.Health, Is.LessThan(target.maxHealth));
        }

        [UnityTest]
        public IEnumerator Torreta_MataAlEnemigoEnSuAlcance()
        {
            var turret = Track(new GameObject("Torreta")).AddComponent<Turret>();
            turret.damage = 5f;
            turret.shotsPerSecond = 20f;
            turret.range = 5f;

            // Velocidad mínima y lejos del objetivo: no llega a atacar antes de morir.
            var enemy = new GameObject().AddComponent<Enemy>();
            enemy.transform.position = new Vector3(3f, 0f);
            enemy.Init(Enemy(health: 10f, speed: 0.1f), Target());

            yield return new WaitForSeconds(0.5f);

            Assert.That(enemy == null, Is.True, "El enemigo debería haber sido destruido");
            Assert.That(Clase14.Enemy.Alive, Is.Empty);
        }
    }
}
