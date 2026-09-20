using System;
using UnityEngine;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    [CreateAssetMenu(fileName = "ScoreEventChannel", menuName = "Clase07/MessageBroker/Score Event Channel")]
    public class ScoreEventChannelSO : ScriptableObject
    {
        public event Action<int> OnRaised;
        public void Raise(int amount) => OnRaised?.Invoke(amount);
    }
}
