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
            RegisterMessaging(builder);

            // RegisterComponentInHierarchy necesita la escena del LifetimeScope, así que
            // queda afuera de RegisterMessaging: los tests de EditMode no tienen jerarquía
            // que recorrer, pero sí pueden verificar el registro del broker.
            builder.RegisterComponentInHierarchy<MessagePipeDemoView>();
        }

        // Los registros que no dependen de la escena viven acá para que los tests
        // ejerciten exactamente el mismo cableado que usa la escena, en vez de duplicarlo.
        public static void RegisterMessaging(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<ScorePickedUpEvent>(options);
        }
    }
}
