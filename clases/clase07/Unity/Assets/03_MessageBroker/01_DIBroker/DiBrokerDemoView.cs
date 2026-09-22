using UnityEngine;
using TMPro;
using VContainer;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.DIBroker
{
    public class DiBrokerDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IMessageBroker _broker;
        private int _total;

        [Inject]
        public void Construct(IMessageBroker broker)
        {
            _broker = broker;
            _broker.Subscribe<ScorePickedUpEvent>(OnScorePickedUp);
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnPublishClicked() => _broker.Publish(new ScorePickedUpEvent(10));

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }
    }
}
