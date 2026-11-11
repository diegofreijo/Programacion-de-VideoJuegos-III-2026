using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Clase14.Tests.EditMode
{
    public class WaveDefinitionTests
    {
        EnemyDefinition m_Rat;
        EnemyDefinition m_Orc;
        WaveDefinition m_Wave;

        [SetUp]
        public void SetUp()
        {
            m_Rat = Enemy("Rata", health: 4, damage: 1, rate: 2);
            m_Orc = Enemy("Orco", health: 16, damage: 3, rate: 1);
            m_Wave = ScriptableObject.CreateInstance<WaveDefinition>();
            m_Wave.entries.Add(new WaveEntry { enemy = m_Rat, count = 4 });
            m_Wave.entries.Add(new WaveEntry { enemy = m_Orc, count = 2 });
            m_Wave.spawnInterval = new FloatRange(0.5f, 1.5f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Wave);
            Object.DestroyImmediate(m_Rat);
            Object.DestroyImmediate(m_Orc);
        }

        static EnemyDefinition Enemy(string name, float health, float damage, float rate)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = name;
            enemy.displayName = name;
            enemy.maxHealth = health;
            enemy.damage = damage;
            enemy.attacksPerSecond = rate;
            return enemy;
        }

        [Test]
        public void Totales_SumanCadaFila()
        {
            Assert.That(m_Wave.TotalEnemies, Is.EqualTo(6));
            Assert.That(m_Wave.TotalHealth, Is.EqualTo(4 * 4 + 2 * 16));
            Assert.That(m_Wave.TotalDps, Is.EqualTo(4 * 2 + 2 * 3));
        }

        [Test]
        public void DuracionEstimada_EsIntervalosEntreEnemigosPorPromedio()
        {
            // 6 enemigos: 5 esperas de 1 s en promedio.
            Assert.That(m_Wave.EstimatedSpawnDuration, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void SpawnOrder_RespetaElOrdenYLaCantidad()
        {
            var order = m_Wave.SpawnOrder().ToList();
            Assert.That(order.Count, Is.EqualTo(6));
            Assert.That(order.Take(4), Is.All.SameAs(m_Rat));
            Assert.That(order.Skip(4), Is.All.SameAs(m_Orc));
        }

        [Test]
        public void Validate_FilaSinEnemigo_LoMarca()
        {
            m_Wave.entries.Add(new WaveEntry { enemy = null, count = 3 });
            Assert.That(m_Wave.Validate(), Has.Some.Contains("Fila 3"));
        }

        [Test]
        public void Validate_EnemigoInvalido_MarcaLaOleada()
        {
            m_Orc.damage = 0f;
            Assert.That(m_Wave.IsValid, Is.False);
        }
    }
}
