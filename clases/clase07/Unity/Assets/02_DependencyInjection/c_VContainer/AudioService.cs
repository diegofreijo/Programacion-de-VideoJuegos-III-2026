using UnityEngine;

namespace Clase07.DI.VContainerExample
{
    // Un juego real usaría AudioSource.PlayOneShot; un log alcanza para no
    // depender de assets de audio en un ejemplo didáctico.
    public class AudioService : IAudioService
    {
        public void PlayCoinSound(string origen = "") => Debug.Log(origen + "[AudioService] coin!");
    }
}
