using System;

namespace Clase07.DI.Shared
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        event Action<int> OnScoreChanged;
        void AddScore(int amount);
    }
}
