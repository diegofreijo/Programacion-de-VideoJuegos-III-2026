using NUnit.Framework;
using MessagePipe;
using VContainer;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.Tests
{
    public class MessagePipeWiringTests
    {
        [Test]
        public void RegisteredContainer_DeliversPublishedMessageToSubscriber()
        {
            var builder = new ContainerBuilder();
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<ScorePickedUpEvent>(options);

            using var container = builder.Build();

            var publisher = container.Resolve<IPublisher<ScorePickedUpEvent>>();
            var subscriber = container.Resolve<ISubscriber<ScorePickedUpEvent>>();

            ScorePickedUpEvent? received = null;
            using var subscription = subscriber.Subscribe(e => received = e);

            publisher.Publish(new ScorePickedUpEvent(10));

            Assert.AreEqual(10, received.Value.Amount);
        }
    }
}
