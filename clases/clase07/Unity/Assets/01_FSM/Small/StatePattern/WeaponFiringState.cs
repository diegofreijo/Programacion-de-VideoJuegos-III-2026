using Clase07.FSM.Core;

namespace Clase07.FSM.Small.StatePattern
{
    public class WeaponFiringState : IState
    {
        private readonly WeaponContext _context;
        private readonly WeaponStatePatternController _controller;

        public WeaponFiringState(WeaponContext context, WeaponStatePatternController controller)
        {
            _context = context;
            _controller = controller;
        }

        public void OnEnter()
        {
            _context.Timer = 0f;
            _context.AmmoInMagazine--;
            _context.ShotsFired++;
        }

        public void OnUpdate(float deltaTime)
        {
            _context.Timer += deltaTime;
            if (_context.Timer >= WeaponContext.FireDuration)
            {
                _controller.ChangeState(_controller.IdleState);
            }
        }

        public void OnExit() { }
    }
}
