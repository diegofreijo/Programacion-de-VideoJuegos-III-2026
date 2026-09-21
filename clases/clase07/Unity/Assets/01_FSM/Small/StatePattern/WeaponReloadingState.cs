using Clase07.FSM.Core;

namespace Clase07.FSM.Small.StatePattern
{
    public class WeaponReloadingState : IState
    {
        private readonly WeaponContext _context;
        private readonly WeaponStatePatternController _controller;

        public WeaponReloadingState(WeaponContext context, WeaponStatePatternController controller)
        {
            _context = context;
            _controller = controller;
        }

        public void OnEnter() => _context.Timer = 0f;

        public void OnUpdate(float deltaTime)
        {
            _context.Timer += deltaTime;
            if (_context.Timer >= WeaponContext.ReloadDuration)
            {
                _context.AmmoInMagazine = WeaponContext.MagazineSize;
                _controller.ChangeState(_controller.IdleState);
            }
        }

        public void OnExit() { }
    }
}
