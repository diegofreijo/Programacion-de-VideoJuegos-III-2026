namespace Clase07.FSM.SmallStatePattern
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

            // La transición vive acá, en el estado que la dispara — no hay un
            // switch central que decida "si estoy en Idle y aprietan, ¿a dónde
            // voy?": cada IState sabe a qué otro estado puede pasar.
            _controller.ChangeState(_context.AmmoInMagazine > 0
                ? _controller.FiringState
                : _controller.ReloadingState);
        }

        public void OnExit() { }
    }
}
