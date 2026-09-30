using UnityEngine;

namespace Clase08
{
    public class Puerta : MonoBehaviour
    {
        public string LlaveNecesaria = "llave-roja";
        public bool Abierta { get; private set; }

        public bool IntentarAbrir(Inventario inventario)
        {
            if (!inventario.TieneLlave(LlaveNecesaria))
                return false;

            Abierta = true;
            return true;
        }
    }
}
