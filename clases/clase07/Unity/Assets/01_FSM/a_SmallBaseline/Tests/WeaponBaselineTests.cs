using NUnit.Framework;
using Clase07.FSM.SmallBaseline;

namespace Clase07.FSM.Tests
{
    public class WeaponBaselineTests
    {
        [Test]
        public void PressTrigger_FromIdle_MovesToFiringAndConsumesAmmo()
        {
            var weapon = new WeaponBaseline();

            weapon.PressTrigger();

            Assert.AreEqual(WeaponState.Firing, weapon.State);
            Assert.AreEqual(WeaponBaseline.MagazineSize - 1, weapon.AmmoInMagazine);
            Assert.AreEqual(1, weapon.ShotsFired);
        }

        [Test]
        public void Tick_PastFireDuration_ReturnsToIdle()
        {
            var weapon = new WeaponBaseline();
            weapon.PressTrigger();

            weapon.Tick(WeaponBaseline.FireDuration + 0.01f);

            Assert.AreEqual(WeaponState.Idle, weapon.State);
        }

        [Test]
        public void PressTrigger_WithEmptyMagazine_StartsReloading()
        {
            var weapon = new WeaponBaseline();
            for (var i = 0; i < WeaponBaseline.MagazineSize; i++)
            {
                weapon.PressTrigger();
                weapon.Tick(WeaponBaseline.FireDuration + 0.01f);
            }

            weapon.PressTrigger();

            Assert.AreEqual(WeaponState.Reloading, weapon.State);
        }

        [Test]
        public void Tick_PastReloadDuration_RefillsMagazineAndReturnsToIdle()
        {
            var weapon = new WeaponBaseline();
            for (var i = 0; i < WeaponBaseline.MagazineSize; i++)
            {
                weapon.PressTrigger();
                weapon.Tick(WeaponBaseline.FireDuration + 0.01f);
            }
            weapon.PressTrigger();

            weapon.Tick(WeaponBaseline.ReloadDuration + 0.01f);

            Assert.AreEqual(WeaponState.Idle, weapon.State);
            Assert.AreEqual(WeaponBaseline.MagazineSize, weapon.AmmoInMagazine);
        }
    }
}
