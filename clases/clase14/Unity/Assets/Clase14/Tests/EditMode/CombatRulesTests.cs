using NUnit.Framework;
using UnityEngine;

namespace Clase14.Tests.EditMode
{
    public class CombatRulesTests
    {
        EnemyDefinition m_Enemy;

        [SetUp]
        public void SetUp()
        {
            m_Enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            m_Enemy.maxHealth = 10f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(m_Enemy);

        [TestCase(10f, 4f, 6f)]
        [TestCase(3f, 4f, 0f)]
        [TestCase(10f, -5f, 10f)]
        public void HealthAfterHit_RestaSinBajarDeCero(float current, float damage, float expected)
        {
            Assert.That(CombatRules.HealthAfterHit(m_Enemy, current, damage), Is.EqualTo(expected));
        }

        [Test]
        public void SecondsToKill_CuentaDisparosEnteros()
        {
            // 10 de vida con 4 de daño son 3 disparos; a 2 disparos por segundo, 1.5 s.
            Assert.That(CombatRules.SecondsToKill(m_Enemy, 4f, 2f), Is.EqualTo(1.5f).Within(0.001f));
        }

        [Test]
        public void SecondsToKill_SinDanio_EsInfinito()
        {
            Assert.That(CombatRules.SecondsToKill(m_Enemy, 0f, 2f), Is.EqualTo(float.PositiveInfinity));
        }
    }
}
