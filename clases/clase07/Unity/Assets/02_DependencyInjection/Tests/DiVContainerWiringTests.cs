using NUnit.Framework;
using VContainer;
using Clase07.DI.Shared;

namespace Clase07.DI.Tests
{
    public class DiVContainerWiringTests
    {
        [Test]
        public void ContainerBuilder_ResolvesScoreAndAudioServices()
        {
            var builder = new ContainerBuilder();
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);

            using var container = builder.Build();

            Assert.IsInstanceOf<ScoreService>(container.Resolve<IScoreService>());
            Assert.IsInstanceOf<AudioService>(container.Resolve<IAudioService>());
        }

        [Test]
        public void ContainerBuilder_SingletonLifetime_ReturnsSameInstance()
        {
            var builder = new ContainerBuilder();
            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);

            using var container = builder.Build();

            Assert.AreSame(container.Resolve<IScoreService>(), container.Resolve<IScoreService>());
        }
    }
}
