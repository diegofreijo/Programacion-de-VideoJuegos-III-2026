using System;
using UnityEngine;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    [CreateAssetMenu(fileName = "ScoreEventChannel", menuName = "Clase07/MessageBroker/Score Event Channel")]
    public class ScoreEventChannelSO : ScriptableObject
    {
        public event Action<ScorePickedUpEvent> OnRaised;
        public void Raise(ScorePickedUpEvent evt) => OnRaised?.Invoke(evt);
    }
}
