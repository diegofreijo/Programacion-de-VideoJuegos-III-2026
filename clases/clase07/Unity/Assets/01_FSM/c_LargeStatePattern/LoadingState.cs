namespace Clase07.FSM.LargeStatePattern
{
    public class LoadingState : IGameFlowState
    {
        public string Name => "Loading";
        private readonly GameFlowController _controller;
        public LoadingState(GameFlowController controller) => _controller = controller;
        public void OnEnter() { }
        public void OnUpdate(float deltaTime) { }
        public void OnExit() { }
    }
}
