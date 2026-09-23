using UnityEngine;
using TMPro;
using VContainer;

namespace Clase07.DI.VContainerExample
{
    public class CoinPickupVContainer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IScoreService _scoreService;
        private IAudioService _audioService;

        // La dependencia se DECLARA acá, no se busca (Instance estático) ni se
        // pide bajo demanda (ServiceLocator.Resolve<T>()) — VContainer la
        // resuelve y la pasa antes de que el objeto se use, así que además
        // queda trivial de testear con un fake, sin tocar el contenedor.
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
