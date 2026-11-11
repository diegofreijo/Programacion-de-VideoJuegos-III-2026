using System.Linq;
using Clase14.Editor;
using NUnit.Framework;
using UnityEditor;

namespace Clase14.Tests.EditMode
{
    /// <summary>Revisa los assets reales del proyecto, como lo haría un diseñador con la ventana de balanceo.</summary>
    public class SampleDataTests
    {
        [Test]
        public void LasOleadasDelJuegoPrincipal_SonValidas()
        {
            foreach (var path in new[] { "Assets/Clase14/Data/Oleadas/Oleada1.asset", "Assets/Clase14/Data/Oleadas/Oleada2.asset" })
            {
                var wave = AssetDatabase.LoadAssetAtPath<WaveDefinition>(path);
                Assert.That(wave, Is.Not.Null, path);
                Assert.That(wave.Validate(), Is.Empty, path);
            }
        }

        [Test]
        public void LaVentanaDeBalanceo_EncuentraTodosLosEnemigos()
        {
            var names = BalanceWindow.FindAllEnemies().Select(e => e.name).ToList();
            Assert.That(names, Is.SupersetOf(new[] { "Rata", "Orco", "Golem", "Berserker", "Fantasma" }));
        }
    }
}
