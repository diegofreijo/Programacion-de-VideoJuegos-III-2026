using UnityEngine;

namespace Clase14
{
    /// <summary>Lo que los enemigos atacan en el centro del mapa. Si su vida llega a 0, se pierde.</summary>
    public class DefendedTarget : MonoBehaviour
    {
        [Min(1)] public float maxHealth = 100f;
        [Tooltip("Distancia desde la que un enemigo puede golpear.")]
        [Min(0.1f)] public float hitRadius = 0.75f;

        public float Health { get; private set; }
        public bool IsDestroyed => Health <= 0f;

        void Awake()
        {
            Health = maxHealth;
            if (GetComponent<SpriteRenderer>() == null)
                SpriteFactory.AddRenderer(gameObject, SpriteFactory.Circle, new Color(0.3f, 0.8f, 1f), 1);
        }

        public void ReceiveDamage(float amount)
        {
            if (IsDestroyed) return;
            Health = Mathf.Max(0f, Health - Mathf.Max(0f, amount));
        }
    }
}
