using NUnit.Framework;
using Clase07.FSM.SmallStatePattern;

namespace Clase07.FSM.Tests
{
    // Corre la misma secuencia de inputs que WeaponBaselineExpectedSequenceTests
    // en a_SmallBaseline/Tests/ — a propósito NO referencia ese tipo: cada
    // implementación se verifica contra los mismos valores esperados hardcodeados
    // en vez de compararse en runtime, para que esta carpeta compile sola.
    public class WeaponStatePatternExpectedSequenceTests
    {
        private static string StateName(WeaponStatePatternController c)
        {
            if (c.CurrentState == c.IdleState) return "Idle";
            if (c.CurrentState == c.FiringState) return "Firing";
            return "Reloading";
        }

        [Test]
        public void FiringUntilEmptyThenReloading_MatchesExpectedAmmoAndStateSequence()
        {
            var weapon = new WeaponStatePatternController();
            var expectedAmmoAfterShot = new[] { 5, 4, 3, 2, 1, 0 };

            for (var shot = 0; shot < WeaponContext.MagazineSize; shot++)
            {
                weapon.PressTrigger();
                weapon.Tick(0f);

                Assert.AreEqual("Firing", StateName(weapon));
                Assert.AreEqual(expectedAmmoAfterShot[shot], weapon.Context.AmmoInMagazine);

                weapon.Tick(WeaponContext.FireDuration + 0.01f);
                Assert.AreEqual("Idle", StateName(weapon));
            }

            weapon.PressTrigger();
            weapon.Tick(0f);
            Assert.AreEqual("Reloading", StateName(weapon));
            Assert.AreEqual(0, weapon.Context.AmmoInMagazine);

            weapon.Tick(WeaponContext.ReloadDuration + 0.01f);
            Assert.AreEqual("Idle", StateName(weapon));
            Assert.AreEqual(WeaponContext.MagazineSize, weapon.Context.AmmoInMagazine);
        }
    }
}
