using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Clase14
{
    /// <summary>Un enemigo en escena: camina hacia el objetivo y lo golpea. Sus números salen de un EnemyDefinition.</summary>
    public class Enemy : MonoBehaviour
    {
        /// <summary>Todos los enemigos vivos. La torreta busca acá sin FindObjectsByType.</summary>
        public static readonly List<Enemy> Alive = new List<Enemy>();

        public EnemyDefinition Definition { get; private set; }
        public float Health { get; private set; }
        public bool IsDead => Health <= 0f;

        DefendedTarget m_Target;
        float m_AttackCooldown;
        bool m_Careless;

        /// <summary>
        /// Prepara al enemigo. Con <paramref name="careless"/> busca el objetivo cada frame de la
        /// peor manera posible: así se ven los picos de GC en el Profiler.
        /// </summary>
        public void Init(EnemyDefinition definition, DefendedTarget target, bool careless = false)
        {
            Definition = definition;
            Health = definition.maxHealth;
            m_Target = target;
            m_Careless = careless;
            name = $"Enemigo ({definition.displayName})";
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
                renderer = SpriteFactory.AddRenderer(gameObject, SpriteFactory.Square, definition.color, 2);
            renderer.color = definition.color;
            transform.localScale = Vector3.one * Mathf.Lerp(0.35f, 0.8f, Mathf.InverseLerp(1f, 60f, definition.maxHealth));
        }

        void OnEnable() => Alive.Add(this);
        void OnDisable() => Alive.Remove(this);

        void Update()
        {
            if (Definition == null) return;
            if (m_Careless)
                m_Target = FindTargetCarelessly();
            if (m_Target == null || m_Target.IsDestroyed) return;

            var toTarget = m_Target.transform.position - transform.position;
            if (toTarget.magnitude > m_Target.hitRadius)
            {
                transform.position += toTarget.normalized * (Definition.moveSpeed * Time.deltaTime);
                return;
            }

            m_AttackCooldown -= Time.deltaTime;
            if (m_AttackCooldown <= 0f)
            {
                m_Target.ReceiveDamage(Definition.damage);
                m_AttackCooldown = 1f / Definition.attacksPerSecond;
            }
        }

        // Mala práctica a propósito, para la parte del Profiler: cada enemigo, cada frame, crea
        // arrays, listas de LINQ y strings que el recolector de basura tiene que limpiar.
        DefendedTarget FindTargetCarelessly()
        {
            var targets = FindObjectsByType<DefendedTarget>();
            var closest = targets
                .Where(t => !t.IsDestroyed)
                .OrderBy(t => Vector3.Distance(t.transform.position, transform.position))
                .ToList();
            name = "Enemigo " + Definition.displayName + " buscando " + closest.Count + " objetivos";
            return closest.FirstOrDefault();
        }

        public void TakeHit(float damage)
        {
            if (IsDead) return;
            Health = CombatRules.HealthAfterHit(Definition, Health, damage);
            if (IsDead)
                Destroy(gameObject);
        }
    }
}
