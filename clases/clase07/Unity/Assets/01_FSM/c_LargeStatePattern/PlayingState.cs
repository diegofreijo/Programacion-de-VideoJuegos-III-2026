namespace Clase07.FSM.LargeStatePattern
{
    public class PlayingState : IGameFlowState
    {
        public string Name => "Playing";
        public string CurrentSubstateName => _substateMachine.CurrentState.Name;

        public UserPlayingState UserPlaying { get; }
        public PauseMenuState PauseMenu { get; }
        public SettingsMenuState SettingsMenu { get; }

        // Una StateMachine adentro de un estado: Playing es, para la máquina de
        // arriba, un único estado — pero puertas adentro tiene su propia
        // sub-máquina (UserPlaying → PauseMenu → SettingsMenu) que arranca en
        // OnEnter() y se tickea desde OnUpdate(). Los guards "!= null" de Pause/
        // Resume/OpenSettings/CloseSettings existen porque GameFlowController
        // expone esos métodos incondicionalmente y una escena con botones
        // independientes los puede disparar antes de que Play() haya creado esta
        // sub-máquina.
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
