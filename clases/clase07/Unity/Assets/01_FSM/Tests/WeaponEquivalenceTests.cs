using NUnit.Framework;
using Clase07.FSM.Small.Baseline;
using Clase07.FSM.Small.StatePattern;

namespace Clase07.FSM.Tests
{
    public class WeaponEquivalenceTests
    {
        private static string StateName(WeaponStatePatternController c)
        {
            if (c.CurrentState == c.IdleState) return "Idle";
            if (c.CurrentState == c.FiringState) return "Firing";
            return "Reloading";
        }

        [Test]
        public void BothImplementations_MatchAfterFiringUntilEmptyAndReloading()
        {
            var baseline = new WeaponBaseline();
            var statePattern = new WeaponStatePatternController();

            for (var shot = 0; shot < WeaponBaseline.MagazineSize + 1; shot++)
            {
                baseline.PressTrigger();
                statePattern.PressTrigger();
                statePattern.Tick(0f);

                Assert.AreEqual(baseline.State.ToString(), StateName(statePattern));
                Assert.AreEqual(baseline.AmmoInMagazine, statePattern.Context.AmmoInMagazine);

                baseline.Tick(WeaponBaseline.FireDuration + 0.01f);
                statePattern.Tick(WeaponBaseline.FireDuration + 0.01f);
            }

            baseline.Tick(WeaponBaseline.ReloadDuration + 0.01f);
            statePattern.Tick(WeaponBaseline.ReloadDuration + 0.01f);

            Assert.AreEqual(baseline.State.ToString(), StateName(statePattern));
            Assert.AreEqual(baseline.AmmoInMagazine, statePattern.Context.AmmoInMagazine);
        }
    }
}
