using UnityEngine;
using TMPro;

namespace Clase07.DI.ServiceLocatorPattern
{
    public class DiServiceLocatorDemoBootstrapper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private void Awake()
        {
            ServiceLocatorCompositionRoot.Bootstrap();
            _scoreLabel.text = "Score: 0";
        }

        public void OnCoinClicked()
        {
            // El consumidor pide el servicio por tipo en el momento en que lo
            // necesita — a diferencia de VContainer (c_VContainer/), esta
            // dependencia no aparece en ningún constructor ni firma pública.
            var scoreService = ServiceLocator.Resolve<IScoreService>();
            ServiceLocator.Resolve<IAudioService>().PlayCoinSound();
            scoreService.AddScore(10);
            _scoreLabel.text = $"Score: {scoreService.CurrentScore}";
        }
    }
}
