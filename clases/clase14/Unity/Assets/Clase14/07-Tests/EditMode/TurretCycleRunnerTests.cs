using NUnit.Framework;
using UnityEngine;

namespace Clase14.Tests.EditMode
{
    /// <summary>El recorrido del ciclo, sin grafo ni escena: un asset armado a mano y el runner.</summary>
    public class TurretCycleRunnerTests
    {
        TurretCycleAsset m_Cycle;

        static TurretCycleAsset.Transition To(int next, TurretCondition condition, float value = 0f) =>
            new TurretCycleAsset.Transition { condition = condition, value = value, next = next };

        [SetUp]
        public void SetUp()
        {
            // El mismo ciclo que el de ejemplo: Activa (0), Recalentada (1), Enfriando (2).
            m_Cycle = ScriptableObject.CreateInstance<TurretCycleAsset>();
            m_Cycle.start = 0;
            m_Cycle.states.Add(new TurretCycleAsset.State { name = "Activa", transitions = { To(1, TurretCondition.ShotsFired, 10f) } });
            m_Cycle.states.Add(new TurretCycleAsset.State { name = "Recalentada", canFire = false, transitions = { To(2, TurretCondition.TimeElapsed, 2f) } });
            m_Cycle.states.Add(new TurretCycleAsset.State
            {
                name = "Enfriando",
                fireRateMultiplier = 0.5f,
                transitions = { To(0, TurretCondition.TimeElapsed, 3f), To(0, TurretCondition.NoEnemiesInRange) },
            });
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(m_Cycle);

        static void Shoot(TurretCycleRunner runner, int shots)
        {
            for (var i = 0; i < shots; i++) runner.RegisterShot();
        }

        [Test]
        public void Recorre_ActivaRecalentadaEnfriandoYVuelveAActiva()
        {
            var runner = new TurretCycleRunner(m_Cycle);
            Assert.That(runner.Current.name, Is.EqualTo("Activa"));

            Shoot(runner, 9);
            Assert.That(runner.Tick(0.1f, true), Is.False, "Con 9 disparos sigue Activa");
            Shoot(runner, 1);
            Assert.That(runner.Tick(0.1f, true), Is.True);
            Assert.That(runner.Current.name, Is.EqualTo("Recalentada"));

            runner.Tick(1.9f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Recalentada"), "A los 1,9 s sigue Recalentada");
            runner.Tick(0.1f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Enfriando"));

            runner.Tick(2.9f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Enfriando"));
            runner.Tick(0.1f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Activa"));
        }

        [Test]
        public void SaleDeEnfriando_PorSinEnemigos_AntesDeLos3s()
        {
            var runner = new TurretCycleRunner(m_Cycle);
            Shoot(runner, 10);
            runner.Tick(0f, true);
            runner.Tick(2f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Enfriando"));

            runner.Tick(0.5f, enemiesInRange: false);
            Assert.That(runner.Current.name, Is.EqualTo("Activa"));
        }

        [Test]
        public void LosDisparosCuentanDesdeQueSeEntroAlEstado()
        {
            var runner = new TurretCycleRunner(m_Cycle);
            Shoot(runner, 10);
            runner.Tick(0f, true);   // Recalentada
            runner.Tick(2f, true);   // Enfriando
            Shoot(runner, 10);       // Estos disparos son de Enfriando
            runner.Tick(0f, false);  // Activa, con el contador en cero
            Assert.That(runner.Current.name, Is.EqualTo("Activa"));
            Assert.That(runner.ShotsInState, Is.EqualTo(0));
            runner.Tick(0.1f, true);
            Assert.That(runner.Current.name, Is.EqualTo("Activa"));
        }
    }
}
