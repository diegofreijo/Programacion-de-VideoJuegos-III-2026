using UnityEngine;
using Clase07.DI.Shared;

namespace Clase07.DI.Singleton
{
    public class ScoreServiceSingleton : MonoBehaviour
    {
        public static ScoreServiceSingleton Instance { get; private set; }
        public IScoreService Service { get; private set; }

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Service ??= new ScoreService();
        }
    }
}
