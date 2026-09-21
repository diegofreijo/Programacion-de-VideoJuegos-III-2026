namespace Clase07.FSM.Small.StatePattern
{
    public class WeaponContext
    {
        public const float FireDuration = 0.2f;
        public const float ReloadDuration = 1.0f;
        public const int MagazineSize = 6;

        public int AmmoInMagazine = MagazineSize;
        public int ShotsFired;
        public float Timer;
        public bool TriggerPressedThisFrame;
    }
}
