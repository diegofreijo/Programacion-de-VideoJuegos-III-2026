namespace Clase07.FSM.LargeStatePattern
{
    public class PauseMenuState : IGameFlowState
    {
        public string Name => "PauseMenu";
        private readonly PlayingState _parent;
        public PauseMenuState(PlayingState parent) => _parent = parent;
        public void OnEnter() { }
        public void OnUpdate(float deltaTime) { }
        public void OnExit() { }
    }
}
