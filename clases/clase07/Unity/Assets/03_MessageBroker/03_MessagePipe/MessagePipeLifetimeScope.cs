using MessagePipe;
using VContainer;
using VContainer.Unity;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<ScorePickedUpEvent>(options);
            builder.RegisterComponentInHierarchy<MessagePipeDemoView>();
        }
    }
}
