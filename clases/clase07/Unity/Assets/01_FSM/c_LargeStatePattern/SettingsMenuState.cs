namespace Clase07.FSM.LargeStatePattern
{
    public class SettingsMenuState : IGameFlowState
    {
        public string Name => "SettingsMenu";
        private readonly PlayingState _parent;
        public SettingsMenuState(PlayingState parent) => _parent = parent;
        public void OnEnter() { }
        public void OnUpdate(float deltaTime) { }
        public void OnExit() { }
    }
}
