using UnityEngine;
using TMPro;

namespace Clase07.FSM.LargeScriptableObjectStates
{
    public class FsmGameFlowSoDemoView : MonoBehaviour
    {
        [SerializeField] private LoadingStateSO _loading;
        [SerializeField] private MainMenuStateSO _mainMenu;
        [SerializeField] private PlayingStateSO _playing;
        [SerializeField] private TMP_Text _stateLabel;

        private GameFlowSORunner _flow;

        private void Awake()
        {
            _flow = new GameFlowSORunner();
            _flow.Initialize(_loading, _mainMenu, _playing);
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
