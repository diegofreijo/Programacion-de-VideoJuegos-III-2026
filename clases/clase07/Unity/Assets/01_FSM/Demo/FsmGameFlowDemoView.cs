using UnityEngine;
using TMPro;
using Clase07.FSM.Large.StatePattern;

namespace Clase07.FSM.Demo
{
    public class FsmGameFlowDemoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _stateLabel;

        private GameFlowController _flow;

        private void Awake()
        {
            _flow = new GameFlowController();
            Refresh();
        }

        public void OnFinishLoadingClicked() { _flow.FinishLoading(); Refresh(); }
        public void OnPlayClicked() { _flow.Play(); Refresh(); }
        public void OnPauseClicked() { _flow.Pause(); Refresh(); }
        public void OnOpenSettingsClicked() { _flow.OpenSettings(); Refresh(); }
        public void OnCloseSettingsClicked() { _flow.CloseSettings(); Refresh(); }
        public void OnResumeClicked() { _flow.Resume(); Refresh(); }

        private void Refresh()
        {
            _stateLabel.text = $"State: {_flow.CurrentState.Name} | Substate: {_flow.CurrentSubstateName}";
        }
    }
}
