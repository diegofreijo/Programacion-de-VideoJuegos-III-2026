using NUnit.Framework;
using MessagePipe;
using VContainer;

namespace Clase07.MessageBroker.MessagePipeExample.Tests
{
    // El test llama al mismo MessagePipeLifetimeScope.RegisterMessaging que usa la
    // escena: si alguien rompe ese cableado, este test lo detecta.
    public class MessagePipeWiringTests
    {
        [Test]
        public void LifetimeScopeRegistrations_DeliverPublishedMessageToSubscriber()
        {
            var builder = new ContainerBuilder();
            MessagePipeLifetimeScope.RegisterMessaging(builder);

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
