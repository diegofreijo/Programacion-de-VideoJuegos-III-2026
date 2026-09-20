using VContainer;
using VContainer.Unity;
using Clase07.DI.Shared;

namespace Clase07.DI.VContainerExample
{
    public class DiVContainerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<CoinPickupVContainer>();
        }
    }
}
