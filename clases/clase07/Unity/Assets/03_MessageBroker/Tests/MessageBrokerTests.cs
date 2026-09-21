using NUnit.Framework;
using Clase07.MessageBroker.DIBroker;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.Tests
{
    public class MessageBrokerTests
    {
        [Test]
        public void Publish_DeliversToSubscriber()
        {
            var broker = new Clase07.MessageBroker.DIBroker.MessageBroker();
            ScorePickedUpEvent? received = null;
            broker.Subscribe<ScorePickedUpEvent>(e => received = e);

            broker.Publish(new ScorePickedUpEvent(10));

            Assert.AreEqual(10, received.Value.Amount);
        }

        [Test]
        public void Publish_DeliversToMultipleSubscribers()
        {
            var broker = new Clase07.MessageBroker.DIBroker.MessageBroker();
            var count = 0;
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Dispose_CancelsSubscription()
        {
            var broker = new Clase07.MessageBroker.DIBroker.MessageBroker();
            var count = 0;
            var subscription = broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            subscription.Dispose();
            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            var broker = new Clase07.MessageBroker.DIBroker.MessageBroker();
            Assert.DoesNotThrow(() => broker.Publish(new PlayerDamagedEvent(5)));
        }
    }
}
