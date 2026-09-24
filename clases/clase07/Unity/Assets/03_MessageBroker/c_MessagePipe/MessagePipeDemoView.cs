using System;
using UnityEngine;
using TMPro;
using VContainer;
using MessagePipe;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _scoreLabel;

        private IPublisher<ScorePickedUpEvent> _publisher;
        private IDisposable _subscription;
        private int _total;

        // Mismo problema que a_DIBroker.IMessageBroker (Subscribe/Publish genéricos),
        // pero resuelto por MessagePipe: se inyectan interfaces específicas por tipo de
        // mensaje (IPublisher<T>/ISubscriber<T>) en vez de un broker genérico propio.
        [Inject]
        public void Construct(
            IPublisher<ScorePickedUpEvent> publisher, 
            ISubscriber<ScorePickedUpEvent> subscriber)
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
