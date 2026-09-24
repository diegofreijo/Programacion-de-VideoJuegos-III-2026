using System;
using VContainer;

namespace Clase07.DI.VContainerExample
{
    public class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        [Inject] private IAudioService audioService;

        public void AddScore(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);

            // Ejemplo
            audioService.PlayCoinSound("ScoreService");
        }
    }
}
