using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using MessagePipe;
using Clase07.MessageBroker.Shared;
using Clase07.Shared.UI;

namespace Clase07.MessageBroker.MessagePipeExample
{
    public class MessagePipeDemoView : MonoBehaviour
    {
        private IPublisher<ScorePickedUpEvent> _publisher;
        private IDisposable _subscription;
        private Text _label;
        private int _total;

        [Inject]
        public void Construct(IPublisher<ScorePickedUpEvent> publisher, ISubscriber<ScorePickedUpEvent> subscriber)
        {
            _publisher = publisher;
            _subscription = subscriber.Subscribe(OnScorePickedUp);
        }

        private void Start()
        {
            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Publish Score+10 (MessagePipe)", new Vector2(0, 150));
            button.onClick.AddListener(() => _publisher.Publish(new ScorePickedUpEvent(10)));
            _label = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 110));
        }

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_label != null) _label.text = $"Score: {_total}";
        }

        private void OnDestroy() => _subscription?.Dispose();
    }
}
