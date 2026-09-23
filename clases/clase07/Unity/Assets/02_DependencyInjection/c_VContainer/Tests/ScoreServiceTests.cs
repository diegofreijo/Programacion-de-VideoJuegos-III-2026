using NUnit.Framework;
using Clase07.DI.VContainerExample;

namespace Clase07.DI.VContainerExample.Tests
{
    public class ScoreServiceTests
    {
        [Test]
        public void AddScore_AccumulatesAcrossCalls()
        {
            var service = new ScoreService();
            service.AddScore(10);
            service.AddScore(5);
            Assert.AreEqual(15, service.CurrentScore);
        }

        [Test]
        public void AddScore_FiresOnScoreChangedWithNewTotal()
        {
            var service = new ScoreService();
            int? received = null;
            service.OnScoreChanged += value => received = value;

            service.AddScore(10);

            Assert.AreEqual(10, received);
        }

        [Test]
        public void AddScore_WithNonPositiveAmount_Throws()
        {
            var service = new ScoreService();
            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.AddScore(0));
        }
    }
}
