namespace Clase07.FSM.Large.StatePattern
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
