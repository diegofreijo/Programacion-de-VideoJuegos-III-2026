using System;

namespace Clase07.DI.Singleton
{
    public class ScoreService : IScoreService
    {
        public int CurrentScore { get; private set; }
        public event Action<int> OnScoreChanged;

        public void AddScore(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentScore += amount;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
