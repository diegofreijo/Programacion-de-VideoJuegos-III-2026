using UnityEngine;
using TMPro;
using VContainer;
using Clase07.DI.Shared;

namespace Clase07.DI.VContainerExample
{
    public class CoinPickupVContainer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IScoreService _scoreService;
        private IAudioService _audioService;

        [Inject]
        public void Construct(IScoreService scoreService, IAudioService audioService)
        {
            _scoreService = scoreService;
            _audioService = audioService;
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnCoinClicked()
        {
            _scoreService.AddScore(10);
            _audioService.PlayCoinSound();
            _scoreLabel.text = $"Score: {_scoreService.CurrentScore}";
        }
    }
}
