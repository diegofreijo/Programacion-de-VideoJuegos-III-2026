using UnityEngine;
using UnityEngine.UI;
using Clase07.DI.Shared;
using Clase07.Shared.UI;

namespace Clase07.DI.ServiceLocatorPattern
{
    public class DiServiceLocatorDemoBootstrapper : MonoBehaviour
    {
        private Text _scoreLabel;

        private void Awake() => Build();

        public void Build()
        {
            ServiceLocatorCompositionRoot.Bootstrap();

            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Coin (ServiceLocator)", new Vector2(0, 100));
            _scoreLabel = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 60));

            button.onClick.AddListener(() =>
            {
                var scoreService = ServiceLocator.Resolve<IScoreService>();
                ServiceLocator.Resolve<IAudioService>().PlayCoinSound();
                scoreService.AddScore(10);
                _scoreLabel.text = $"Score: {scoreService.CurrentScore}";
            });
        }
    }
}
