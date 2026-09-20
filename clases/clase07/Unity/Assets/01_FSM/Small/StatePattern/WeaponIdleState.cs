using Clase07.FSM.Core;

namespace Clase07.FSM.Small.StatePattern
{
    public class WeaponIdleState : IState
    {
        private readonly WeaponContext _context;
        private readonly WeaponStatePatternController _controller;

        public WeaponIdleState(WeaponContext context, WeaponStatePatternController controller)
        {
            _context = context;
            _controller = controller;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (!_context.TriggerPressedThisFrame) return;
            _context.TriggerPressedThisFrame = false;

            _controller.ChangeState(_context.AmmoInMagazine > 0
                ? _controller.FiringState
                : _controller.ReloadingState);
        }

        public void OnExit() { }
    }
}
