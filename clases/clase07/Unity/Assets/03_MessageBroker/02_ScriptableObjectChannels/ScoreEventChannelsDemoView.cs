using UnityEngine;
using TMPro;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    public class ScoreEventChannelsDemoView : MonoBehaviour
    {
        [SerializeField] private ScoreEventChannelSO _channel;
        [SerializeField] private TMP_Text _scoreLabel;

        private int _total;

        private void Awake()
        {
            _channel.OnRaised += OnRaised;
            _scoreLabel.text = "Score: 0";
        }

        public void OnPublishClicked() => _channel.Raise(new ScorePickedUpEvent(10));

        private void OnRaised(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.OnRaised -= OnRaised;
        }
    }
}
