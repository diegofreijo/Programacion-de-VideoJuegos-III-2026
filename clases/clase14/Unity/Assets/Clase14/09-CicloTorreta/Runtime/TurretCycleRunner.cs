namespace Clase14
{
    /// <summary>
    /// Recorre un TurretCycleAsset. Clase pura, sin MonoBehaviour: recibe el tiempo, los disparos y
    /// si hay enemigos en alcance, y dice en qué Estado está la torreta. Se testea en Edit Mode.
    /// </summary>
    public class TurretCycleRunner
    {
        readonly TurretCycleAsset m_Asset;

        public int Index { get; private set; }
        /// <summary>Segundos desde que se entró al Estado actual.</summary>
        public float TimeInState { get; private set; }
        /// <summary>Disparos desde que se entró al Estado actual.</summary>
        public int ShotsInState { get; private set; }

        public TurretCycleRunner(TurretCycleAsset asset)
        {
            m_Asset = asset;
            Index = asset != null && asset.start >= 0 && asset.start < asset.states.Count ? asset.start : TurretCycleAsset.None;
        }

        public TurretCycleAsset.State Current => Index == TurretCycleAsset.None ? null : m_Asset.states[Index];

        /// <summary>La torreta avisa cada disparo.</summary>
        public void RegisterShot() => ShotsInState++;

        /// <summary>
        /// Avanza el tiempo y evalúa las transiciones del Estado actual en orden. Cambia como mucho
        /// una vez por llamada. Devuelve true si cambió de Estado.
        /// </summary>
        public bool Tick(float deltaTime, bool enemiesInRange)
        {
            var state = Current;
            if (state == null) return false;
            TimeInState += deltaTime;

            foreach (var transition in state.transitions)
            {
                if (transition.next == TurretCycleAsset.None || !IsMet(transition, enemiesInRange)) continue;
                Index = transition.next;
                TimeInState = 0f;
                ShotsInState = 0;
                return true;
            }
            return false;
        }

        bool IsMet(TurretCycleAsset.Transition transition, bool enemiesInRange)
        {
            switch (transition.condition)
            {
                case TurretCondition.ShotsFired: return ShotsInState >= transition.value;
                case TurretCondition.TimeElapsed: return TimeInState >= transition.value;
                case TurretCondition.NoEnemiesInRange: return !enemiesInRange;
                default: return false;
            }
        }
    }
}
