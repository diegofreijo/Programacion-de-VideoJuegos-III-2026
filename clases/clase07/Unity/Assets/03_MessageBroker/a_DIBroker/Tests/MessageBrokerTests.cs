using NUnit.Framework;
using Clase07.MessageBroker.DIBroker;

namespace Clase07.MessageBroker.DIBroker.Tests
{
    public class MessageBrokerTests
    {
        [Test]
        public void Publish_DeliversToSubscriber()
        {
            var broker = new MessageBroker();
            ScorePickedUpEvent? received = null;
            broker.Subscribe<ScorePickedUpEvent>(e => received = e);

            broker.Publish(new ScorePickedUpEvent(10));

            Assert.AreEqual(10, received.Value.Amount);
        }

        [Test]
        public void Publish_DeliversToMultipleSubscribers()
        {
            var broker = new MessageBroker();
            var count = 0;
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);
            broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(2, count);
        }

        [Test]
        public void Dispose_CancelsSubscription()
        {
            var broker = new MessageBroker();
            var count = 0;
            var subscription = broker.Subscribe<ScorePickedUpEvent>(_ => count++);

            subscription.Dispose();
            broker.Publish(new ScorePickedUpEvent(1));

            Assert.AreEqual(0, count);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            var broker = new MessageBroker();
            Assert.DoesNotThrow(() => broker.Publish(new PlayerDamagedEvent(5)));
        }
    }
}
