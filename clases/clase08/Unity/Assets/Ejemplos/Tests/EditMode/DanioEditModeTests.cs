using NUnit.Framework;

namespace Clase08.Tests.EditMode
{
    public class DanioEditModeTests
    {
        [TestCase(20, 5, 15)]
        [TestCase(20, 20, 0)]
        [TestCase(20, 30, 0)]
        public void Calcular_RestaDefensa_SinProducirDanioNegativo(
            int ataque, int defensa, int esperado)
        {
            // Arrange: los datos llegan como parámetros del TestCase.
            // Act.
            int resultado = CalculadoraDeDanio.Calcular(ataque, defensa);
            // Assert.
            Assert.That(resultado, Is.EqualTo(esperado));
        }
    }
}
