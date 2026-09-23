using UnityEngine;

namespace Clase07.DI.Singleton
{
    public class ScoreServiceSingleton : MonoBehaviour
    {
        // Acceso global mutable: cualquier clase del proyecto puede leer
        // Instance sin que la dependencia quede declarada en su constructor —
        // es justo lo que Service Locator (b_ServiceLocator/) y la inyección
        // por constructor (c_VContainer/) buscan dejar de esconder.
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
