using UnityEngine;
using UnityEngine.UI;
using Clase07.FSM.Large.StatePattern;
using Clase07.Shared.UI;

namespace Clase07.FSM.Demo
{
    public class FsmGameFlowDemoView : MonoBehaviour
    {
        private GameFlowController _flow;
        private Text _label;

        private void Awake() => Build();

        public void Build()
        {
            _flow = new GameFlowController();

            var canvas = DemoUiFactory.CreateCanvas("GameFlowCanvas");
            var y = 100;

            AddButton(canvas, "Finish Loading", ref y, () => _flow.FinishLoading());
            AddButton(canvas, "Play", ref y, () => _flow.Play());
            AddButton(canvas, "Pause", ref y, () => _flow.Pause());
            AddButton(canvas, "Open Settings", ref y, () => _flow.OpenSettings());
            AddButton(canvas, "Close Settings", ref y, () => _flow.CloseSettings());
            AddButton(canvas, "Resume", ref y, () => _flow.Resume());

            _label = DemoUiFactory.CreateLabel(canvas.transform, string.Empty, new Vector2(0, y - 40));
            Refresh();
        }

        private void AddButton(Canvas canvas, string label, ref int y, System.Action onClick)
        {
            var button = DemoUiFactory.CreateButton(canvas.transform, label, new Vector2(0, y));
            button.onClick.AddListener(() => { onClick(); Refresh(); });
            y -= 40;
        }

        private void Refresh()
        {
            _label.text = $"State: {_flow.CurrentState.Name} | Substate: {_flow.CurrentSubstateName}";
        }
    }
}
