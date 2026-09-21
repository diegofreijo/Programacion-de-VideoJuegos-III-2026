using Clase07.FSM.Core;

namespace Clase07.FSM.Large.StatePattern
{
    public class PlayingState : IGameFlowState
    {
        public string Name => "Playing";
        public string CurrentSubstateName => _substateMachine.CurrentState.Name;

        public UserPlayingState UserPlaying { get; }
        public PauseMenuState PauseMenu { get; }
        public SettingsMenuState SettingsMenu { get; }

        private StateMachine<IGameFlowState> _substateMachine;

        public PlayingState(GameFlowController controller)
        {
            UserPlaying = new UserPlayingState(this);
            PauseMenu = new PauseMenuState(this);
            SettingsMenu = new SettingsMenuState(this);
        }

        public void OnEnter() => _substateMachine = new StateMachine<IGameFlowState>(UserPlaying);
        public void OnUpdate(float deltaTime) => _substateMachine.Tick(deltaTime);
        public void OnExit() { }

        public void Pause()
        {
            if (_substateMachine != null && _substateMachine.CurrentState == UserPlaying) _substateMachine.ChangeState(PauseMenu);
        }

        public void Resume()
        {
            if (_substateMachine != null && _substateMachine.CurrentState == PauseMenu) _substateMachine.ChangeState(UserPlaying);
        }

        public void OpenSettings()
        {
            if (_substateMachine != null && _substateMachine.CurrentState == PauseMenu) _substateMachine.ChangeState(SettingsMenu);
        }

        public void CloseSettings()
        {
            if (_substateMachine != null && _substateMachine.CurrentState == SettingsMenu) _substateMachine.ChangeState(PauseMenu);
        }
    }
}
