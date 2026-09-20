namespace Clase07.FSM.Large.StatePattern
{
    public class UserPlayingState : IGameFlowState
    {
        public string Name => "UserPlaying";
        private readonly PlayingState _parent;
        public UserPlayingState(PlayingState parent) => _parent = parent;
        public void OnEnter() { }
        public void OnUpdate(float deltaTime) { }
        public void OnExit() { }
    }
}
