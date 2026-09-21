using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Clase07.DI.Shared;
using Clase07.Shared.UI;

namespace Clase07.DI.VContainerExample
{
    public class CoinPickupVContainer : MonoBehaviour
    {
        private IScoreService _scoreService;
        private IAudioService _audioService;
        private Text _scoreLabel;

        [Inject]
        public void Construct(IScoreService scoreService, IAudioService audioService)
        {
            _scoreService = scoreService;
            _audioService = audioService;
        }

        private void Start()
        {
            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Coin (VContainer)", new Vector2(0, 100));
            _scoreLabel = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 60));

            button.onClick.AddListener(() =>
            {
                _scoreService.AddScore(10);
                _audioService.PlayCoinSound();
                _scoreLabel.text = $"Score: {_scoreService.CurrentScore}";
            });
        }
    }
}
