using VContainer;
using VContainer.Unity;

namespace Clase07.MessageBroker.DIBroker
{
    public class DiBrokerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IMessageBroker, MessageBroker>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<DiBrokerDemoView>();
        }
    }
}
