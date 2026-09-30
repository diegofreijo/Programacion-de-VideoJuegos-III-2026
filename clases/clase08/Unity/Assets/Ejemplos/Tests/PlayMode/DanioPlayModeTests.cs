using NUnit.Framework;

namespace Clase08.Tests.PlayMode
{
    public class DanioPlayModeTests
    {
        [Test]
        public void Calcular_ConAtaque20YDefensa5_Devuelve15()
        {
            // Arrange.
            int ataque = 20;
            int defensa = 5;
            // Act: Play Mode también admite tests síncronos.
            int resultado = CalculadoraDeDanio.Calcular(ataque, defensa);
            // Assert.
            Assert.That(resultado, Is.EqualTo(15));
        }
    }
}
