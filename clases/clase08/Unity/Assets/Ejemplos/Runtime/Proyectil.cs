using UnityEngine;

namespace Clase08
{
    public class Proyectil : MonoBehaviour
    {
        public int Danio = 25;
        private bool impactoAplicado;

        public void Impactar(Salud objetivo)
        {
            if (impactoAplicado || objetivo == null)
                return;

            impactoAplicado = true;
            objetivo.RecibirDanio(Danio);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Impactar(collision.gameObject.GetComponent<Salud>());
        }
    }
}
