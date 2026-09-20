using UnityEngine;
using UnityEngine.UI;
using Clase07.MessageBroker.Shared;
using Clase07.Shared.UI;

namespace Clase07.MessageBroker.ScriptableObjectChannels
{
    public class ScoreEventChannelsDemoView : MonoBehaviour
    {
        [SerializeField] private ScoreEventChannelSO _channel;
        private Text _label;
        private int _total;

        public void SetChannel(ScoreEventChannelSO channel) => _channel = channel;

        private void Awake() => Build();

        public void Build()
        {
            _channel.OnRaised += OnRaised;

            var canvas = DemoUiFactory.CreateCanvas();
            var button = DemoUiFactory.CreateButton(canvas.transform, "Publish Score+10 (SO Channel)", new Vector2(0, 150));
            button.onClick.AddListener(() => _channel.Raise(new ScorePickedUpEvent(10)));
            _label = DemoUiFactory.CreateLabel(canvas.transform, "Score: 0", new Vector2(0, 110));
        }

        private void OnRaised(ScorePickedUpEvent evt)
        {
            _total += evt.Amount;
            if (_label != null) _label.text = $"Score: {_total}";
        }

        private void OnDestroy()
        {
            if (_channel != null) _channel.OnRaised -= OnRaised;
        }
    }
}
