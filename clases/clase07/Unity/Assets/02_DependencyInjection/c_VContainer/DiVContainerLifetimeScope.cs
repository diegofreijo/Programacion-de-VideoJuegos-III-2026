using VContainer;
using VContainer.Unity;

namespace Clase07.DI.VContainerExample
{
    public class DiVContainerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterServices(builder);

            // RegisterComponentInHierarchy necesita la escena del LifetimeScope, así que
            // queda afuera de RegisterServices: los tests de EditMode no tienen jerarquía
            // que recorrer, pero sí pueden verificar los registros de servicios.
            builder.RegisterComponentInHierarchy<CoinPickupVContainer>();
        }

        // Los registros que no dependen de la escena viven acá para que los tests
        // ejerciten exactamente el mismo cableado que usa la escena, en vez de duplicarlo.
        public static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
        }
    }
}
