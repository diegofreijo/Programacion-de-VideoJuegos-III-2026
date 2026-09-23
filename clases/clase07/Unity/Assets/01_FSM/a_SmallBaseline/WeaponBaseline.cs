namespace Clase07.FSM.SmallBaseline
{
    public enum WeaponState { Idle, Firing, Reloading }

    // Implementación "rápida": todo el comportamiento vive en un switch. A
    // propósito no escala bien — cada estado nuevo agranda el switch y es fácil
    // olvidarse un caso.
    public class WeaponBaseline
    {
        public const float FireDuration = 0.2f;
        public const float ReloadDuration = 1.0f;
        public const int MagazineSize = 6;

        public WeaponState State { get; private set; } = WeaponState.Idle;
        public int AmmoInMagazine { get; private set; } = MagazineSize;
        public int ShotsFired { get; private set; }

        private float _timer;

        public void PressTrigger()
        {
            if (State != WeaponState.Idle) return;

            if (AmmoInMagazine > 0)
            {
                State = WeaponState.Firing;
                _timer = 0f;
                AmmoInMagazine--;
                ShotsFired++;
            }
            else
            {
                State = WeaponState.Reloading;
                _timer = 0f;
            }
        }

        public void Tick(float deltaTime)
        {
            // Cada estado nuevo (ej. "Aiming", "Overheated") agrega un case acá
            // y potencialmente en PressTrigger — nada obliga a cubrir todos los
            // casos, a diferencia de un objeto de estado que implementa su propia
            // interfaz.
            switch (State)
            {
                case WeaponState.Idle:
                    break;
                case WeaponState.Firing:
                    _timer += deltaTime;
                    if (_timer >= FireDuration) State = WeaponState.Idle;
                    break;
                case WeaponState.Reloading:
                    _timer += deltaTime;
                    if (_timer >= ReloadDuration)
                    {
                        AmmoInMagazine = MagazineSize;
                        State = WeaponState.Idle;
                    }
                    break;
            }
        }
    }
}
