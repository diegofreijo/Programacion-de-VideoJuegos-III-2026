using System;
using UnityEngine;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    // Cada nuevo listener se conecta arrastrando este asset a un campo
    // [SerializeField] en el Inspector — cero código para agregar un suscriptor
    // nuevo, a diferencia de a_DIBroker/c_MessagePipe donde hace falta inyectar
    // el broker/publisher.
    [CreateAssetMenu(fileName = "ScoreEventChannel", menuName = "Clase07/MessageBroker/Score Event Channel")]
    public class ScoreEventChannelSO : ScriptableObject
    {
        public event Action<ScorePickedUpEvent> OnRaised;
        public void Raise(ScorePickedUpEvent evt) => OnRaised?.Invoke(evt);
    }
}
