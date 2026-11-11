using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Reglas de combate puras, sin MonoBehaviour: se testean en Edit Mode sin escena.
    /// </summary>
    public static class CombatRules
    {
        /// <summary>Vida que le queda a un enemigo después de recibir un golpe.</summary>
        public static float HealthAfterHit(EnemyDefinition enemy, float currentHealth, float incomingDamage)
        {
            var damage = Mathf.Max(0f, incomingDamage);
            return Mathf.Max(0f, currentHealth - damage);
        }

        /// <summary>Segundos que tarda una torreta en matar a un enemigo con la vida completa.</summary>
        public static float SecondsToKill(EnemyDefinition enemy, float turretDamage, float turretShotsPerSecond)
        {
            if (turretDamage <= 0f || turretShotsPerSecond <= 0f) return float.PositiveInfinity;
            var shots = Mathf.Ceil(enemy.maxHealth / turretDamage);
            return shots / turretShotsPerSecond;
        }
    }
}
