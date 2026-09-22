using System;
using UnityEngine;
using TMPro;
using VContainer;
using MessagePipe;
using Clase07.MessageBroker.Shared;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IPublisher<ScorePickedUpEvent> _publisher;
        private IDisposable _subscription;
        private int _total;

        [Inject]
        public void Construct(IPublisher<ScorePickedUpEvent> publisher, ISubscriber<ScorePickedUpEvent> subscriber)
        {
            _publisher = publisher;
            _subscription = subscriber.Subscribe(OnScorePickedUp);
        }

        private void Start() => _scoreLabel.text = "Score: 0";

        public void OnPublishClicked() => _publisher.Publish(new ScorePickedUpEvent(10));

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_scoreLabel != null) _scoreLabel.text = $"Score: {_total}";
        }

        private void OnDestroy() => _subscription?.Dispose();
    }
}
