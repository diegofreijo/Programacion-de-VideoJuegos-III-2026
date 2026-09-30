using UnityEngine;

namespace Clase08
{
    public class Salud : MonoBehaviour
    {
        public int Puntos { get; private set; } = 100;

        public void RecibirDanio(int cantidad)
        {
            Puntos = Mathf.Max(0, Puntos - cantidad);
        }
    }
}
