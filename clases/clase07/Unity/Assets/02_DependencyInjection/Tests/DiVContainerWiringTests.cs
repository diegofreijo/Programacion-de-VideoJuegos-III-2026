using NUnit.Framework;
using VContainer;
using Clase07.DI.Shared;
using Clase07.DI.VContainerExample;

namespace Clase07.DI.Tests
{
    // Los tests llaman al mismo DiVContainerLifetimeScope.RegisterServices que usa la
    // escena: si alguien rompe ese cableado, estos tests lo detectan.
    public class DiVContainerWiringTests
    {
        [Test]
        public void LifetimeScopeRegistrations_ResolveScoreAndAudioServices()
        {
            var builder = new ContainerBuilder();
            DiVContainerLifetimeScope.RegisterServices(builder);

            using var container = builder.Build();

            Assert.IsInstanceOf<ScoreService>(container.Resolve<IScoreService>());
            Assert.IsInstanceOf<AudioService>(container.Resolve<IAudioService>());
        }

        [Test]
        public void LifetimeScopeRegistrations_SingletonLifetime_ReturnsSameInstance()
        {
            var builder = new ContainerBuilder();
            DiVContainerLifetimeScope.RegisterServices(builder);

            using var container = builder.Build();

            Assert.AreSame(container.Resolve<IScoreService>(), container.Resolve<IScoreService>());
            Assert.AreSame(container.Resolve<IAudioService>(), container.Resolve<IAudioService>());
        }
    }
}
