namespace Clase07.FSM.Large.StatePattern
{
    public class MainMenuState : IGameFlowState
    {
        public string Name => "MainMenu";
        private readonly GameFlowController _controller;
        public MainMenuState(GameFlowController controller) => _controller = controller;
        public void OnEnter() { }
        public void OnUpdate(float deltaTime) { }
        public void OnExit() { }
    }
}
