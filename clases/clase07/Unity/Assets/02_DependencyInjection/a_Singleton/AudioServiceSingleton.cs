using UnityEngine;

namespace Clase07.DI.Singleton
{
    public class AudioServiceSingleton : MonoBehaviour
    {
        // Mismo problema que ScoreServiceSingleton.Instance: acceso global
        // mutable, dependencia oculta del consumidor.
        public static AudioServiceSingleton Instance { get; private set; }
        public IAudioService Service { get; private set; }

        private void Awake() => Initialize();

        public void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Service ??= new AudioService();
        }
    }
}
