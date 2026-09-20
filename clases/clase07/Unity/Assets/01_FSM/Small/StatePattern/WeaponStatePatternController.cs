using Clase07.FSM.Core;

namespace Clase07.FSM.Small.StatePattern
{
    public class WeaponStatePatternController
    {
        public WeaponContext Context { get; } = new WeaponContext();
        public IState CurrentState => _machine.CurrentState;
        public IState IdleState { get; }
        public IState FiringState { get; }
        public IState ReloadingState { get; }

        private readonly StateMachine<IState> _machine;

        public WeaponStatePatternController()
        {
            IdleState = new WeaponIdleState(Context, this);
            FiringState = new WeaponFiringState(Context, this);
            ReloadingState = new WeaponReloadingState(Context, this);
            _machine = new StateMachine<IState>(IdleState);
        }

        public void ChangeState(IState next) => _machine.ChangeState(next);
        public void PressTrigger() => Context.TriggerPressedThisFrame = true;
        public void Tick(float deltaTime) => _machine.Tick(deltaTime);
    }
}
