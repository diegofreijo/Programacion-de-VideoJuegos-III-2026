using NUnit.Framework;
using Clase07.FSM.SmallBaseline;

namespace Clase07.FSM.Tests
{
    // Corre la misma secuencia de inputs que WeaponStatePatternExpectedSequenceTests
    // en b_SmallStatePattern/Tests/ — a propósito NO referencia ese tipo: cada
    // implementación se verifica contra los mismos valores esperados hardcodeados
    // en vez de compararse en runtime, para que esta carpeta compile sola.
    public class WeaponBaselineExpectedSequenceTests
    {
        [Test]
        public void FiringUntilEmptyThenReloading_MatchesExpectedAmmoAndStateSequence()
        {
            var weapon = new WeaponBaseline();
            var expectedAmmoAfterShot = new[] { 5, 4, 3, 2, 1, 0 };

            for (var shot = 0; shot < WeaponBaseline.MagazineSize; shot++)
            {
                weapon.PressTrigger();

                Assert.AreEqual(WeaponState.Firing, weapon.State);
                Assert.AreEqual(expectedAmmoAfterShot[shot], weapon.AmmoInMagazine);

                weapon.Tick(WeaponBaseline.FireDuration + 0.01f);
                Assert.AreEqual(WeaponState.Idle, weapon.State);
            }

            weapon.PressTrigger();
            Assert.AreEqual(WeaponState.Reloading, weapon.State);
            Assert.AreEqual(0, weapon.AmmoInMagazine);

            weapon.Tick(WeaponBaseline.ReloadDuration + 0.01f);
            Assert.AreEqual(WeaponState.Idle, weapon.State);
            Assert.AreEqual(WeaponBaseline.MagazineSize, weapon.AmmoInMagazine);
        }
    }
}
