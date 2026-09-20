using NUnit.Framework;
using Clase07.DI.ServiceLocatorPattern;

namespace Clase07.DI.Tests
{
    public class ServiceLocatorTests
    {
        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void Register_ThenResolve_ReturnsSameInstance()
        {
            var instance = new object();
            ServiceLocator.Register(instance);

            Assert.AreSame(instance, ServiceLocator.Resolve<object>());
        }

        [Test]
        public void Resolve_WithoutRegistering_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() => ServiceLocator.Resolve<string>());
        }

        [Test]
        public void Clear_RemovesAllRegistrations()
        {
            ServiceLocator.Register("hello");
            ServiceLocator.Clear();

            Assert.Throws<System.InvalidOperationException>(() => ServiceLocator.Resolve<string>());
        }
    }
}
