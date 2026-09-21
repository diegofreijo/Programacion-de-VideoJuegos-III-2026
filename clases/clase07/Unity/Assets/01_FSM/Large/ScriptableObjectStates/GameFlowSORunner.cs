namespace Clase07.FSM.Large.ScriptableObjectStates
{
    public class GameFlowSORunner
    {
        private LoadingStateSO _loading;
        private MainMenuStateSO _mainMenu;
        private PlayingStateSO _playing;
        private GameFlowStateSO _current;

        public GameFlowStateSO CurrentState => _current;
        public string CurrentSubstateName => _current == _playing ? _playing.CurrentSubstateName : "None";

        public void Initialize(LoadingStateSO loading, MainMenuStateSO mainMenu, PlayingStateSO playing)
        {
            _loading = loading;
            _mainMenu = mainMenu;
            _playing = playing;
            ChangeState(_loading);
        }

        public void FinishLoading() => ChangeState(_mainMenu);
        public void Play() => ChangeState(_playing);
        public void Pause() => _playing.Pause();
        public void Resume() => _playing.Resume();
        public void OpenSettings() => _playing.OpenSettings();
        public void CloseSettings() => _playing.CloseSettings();

        private void ChangeState(GameFlowStateSO next)
        {
            if (_current == next) return;
            _current?.Exit();
            _current = next;
            _current.Enter();
        }
    }
}
