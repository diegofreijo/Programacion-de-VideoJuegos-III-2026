using UnityEngine;
using TMPro;
using Clase07.DI.Shared;

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
            var scoreService = ServiceLocator.Resolve<IScoreService>();
            ServiceLocator.Resolve<IAudioService>().PlayCoinSound();
            scoreService.AddScore(10);
            _scoreLabel.text = $"Score: {scoreService.CurrentScore}";
        }
    }
}
