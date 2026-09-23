namespace Clase07.FSM.LargeStatePattern
{
    public class GameFlowController
    {
        public LoadingState Loading { get; }
        public MainMenuState MainMenu { get; }
        public PlayingState Playing { get; }

        public IGameFlowState CurrentState => _machine.CurrentState;
        public string CurrentSubstateName => CurrentState == Playing ? Playing.CurrentSubstateName : "None";

        private readonly StateMachine<IGameFlowState> _machine;

        public GameFlowController()
        {
            Loading = new LoadingState(this);
            MainMenu = new MainMenuState(this);
            Playing = new PlayingState(this);
            _machine = new StateMachine<IGameFlowState>(Loading);
        }

        public void ChangeState(IGameFlowState next) => _machine.ChangeState(next);

        public void Load() => ChangeState(Loading);
        public void FinishLoading() => ChangeState(MainMenu);
        public void Play() => ChangeState(Playing);

        public void Pause() => Playing.Pause();
        public void Resume() => Playing.Resume();
        public void OpenSettings() => Playing.OpenSettings();
        public void CloseSettings() => Playing.CloseSettings();
    }
}
