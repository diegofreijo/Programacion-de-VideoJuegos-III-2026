using NUnit.Framework;
using UnityEngine;

namespace Clase14.Tests.EditMode
{
    public class EnemyDefinitionTests
    {
        EnemyDefinition m_Enemy;

        [SetUp]
        public void SetUp()
        {
            m_Enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            m_Enemy.displayName = "Orco";
            m_Enemy.color = Color.green;
            m_Enemy.maxHealth = 16f;
            m_Enemy.moveSpeed = 2f;
            m_Enemy.damage = 3f;
            m_Enemy.attacksPerSecond = 1f;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(m_Enemy);

        [TestCase(3f, 1f, 3f)]
        [TestCase(2f, 2.5f, 5f)]
        [TestCase(0f, 4f, 0f)]
        public void EstimatedDps_EsDanioPorGolpesPorSegundo(float damage, float rate, float expected)
        {
            m_Enemy.damage = damage;
            m_Enemy.attacksPerSecond = rate;
            Assert.That(m_Enemy.EstimatedDps, Is.EqualTo(expected).Within(0.001f));
        }

        [Test]
        public void Validate_UnEnemigoRazonable_NoTieneProblemas()
        {
            Assert.That(m_Enemy.Validate(), Is.Empty);
        }

        [Test]
        public void Validate_SinDanio_LoMarca()
        {
            m_Enemy.damage = 0f;
            Assert.That(m_Enemy.Validate(), Has.Some.Contains("daño es 0"));
        }

        [Test]
        public void Validate_DpsPorEncimaDelMaximo_LoMarca()
        {
            m_Enemy.damage = 6f;
            m_Enemy.attacksPerSecond = 4f;
            Assert.That(m_Enemy.IsValid, Is.False);
            Assert.That(m_Enemy.Validate(), Has.Some.Contains("DPS estimado"));
        }

        [Test]
        public void Validate_ColorTransparente_LoMarca()
        {
            m_Enemy.color = new Color(1f, 1f, 1f, 0.1f);
            Assert.That(m_Enemy.Validate(), Has.Some.Contains("transparente"));
        }
    }
}
