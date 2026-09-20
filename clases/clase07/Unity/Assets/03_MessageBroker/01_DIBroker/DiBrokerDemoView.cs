using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Clase07.MessageBroker.Shared;
using Clase07.Shared.UI;

namespace Clase07.MessageBroker.DIBroker
{
    public class DiBrokerDemoView : MonoBehaviour
    {
        private IMessageBroker _broker;
        private Text _label;
        private int _total;

        [Inject]
        public void Construct(IMessageBroker broker)
        {
            _broker = broker;
            _broker.Subscribe<ScorePickedUpEvent>(OnScorePickedUp);
        }

        private void Start()
        {
            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Publish Score+10 (DIBroker)", new Vector2(0, 150));
            button.onClick.AddListener(() => _broker.Publish(new ScorePickedUpEvent(10)));
            _label = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 110));
        }

        private void OnScorePickedUp(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_label != null) _label.text = $"Score: {_total}";
        }
    }
}
