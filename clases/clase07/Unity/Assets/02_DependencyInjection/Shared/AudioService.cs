using UnityEngine;

namespace Clase07.DI.Shared
{
    // Un juego real usaría AudioSource.PlayOneShot; un log alcanza para no
    // depender de assets de audio en un ejemplo didáctico.
    public class AudioService : IAudioService
    {
        public void PlayCoinSound() => Debug.Log("[AudioService] coin!");
    }
}
