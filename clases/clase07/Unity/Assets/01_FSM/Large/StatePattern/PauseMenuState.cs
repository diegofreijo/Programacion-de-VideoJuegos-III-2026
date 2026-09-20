namespace Clase07.FSM.Large.StatePattern
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
