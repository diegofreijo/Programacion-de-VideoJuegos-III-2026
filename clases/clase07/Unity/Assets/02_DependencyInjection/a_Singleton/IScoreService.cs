using System;

namespace Clase07.DI.Singleton
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> OnScoreChanged;
        void AddScore(int amount);
    }
}
