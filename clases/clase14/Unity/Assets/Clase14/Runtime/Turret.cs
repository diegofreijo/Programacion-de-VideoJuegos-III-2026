using UnityEngine;

namespace Clase14
{
    /// <summary>Torreta automática: dispara al enemigo vivo más cercano dentro de su alcance.</summary>
    public class Turret : MonoBehaviour
    {
        [Min(0)] public float damage = 4f;
        [Range(0.1f, 20f)] public float shotsPerSecond = 3f;
        [Min(0.5f)] public float range = 6f;

        float m_Cooldown;
        LineRenderer m_Beam;
        float m_BeamTimer;

        void Awake()
        {
            m_Beam = gameObject.AddComponent<LineRenderer>();
            m_Beam.positionCount = 2;
            m_Beam.startWidth = m_Beam.endWidth = 0.05f;
            m_Beam.material = new Material(Shader.Find("Sprites/Default"));
            m_Beam.startColor = m_Beam.endColor = Color.yellow;
            m_Beam.enabled = false;
        }

        void Update()
        {
            m_BeamTimer -= Time.deltaTime;
            if (m_BeamTimer <= 0f) m_Beam.enabled = false;

            m_Cooldown -= Time.deltaTime;
            if (m_Cooldown > 0f) return;

            var target = FindClosestEnemy();
            if (target == null) return;

            m_Beam.SetPosition(0, transform.position);
            m_Beam.SetPosition(1, target.transform.position);
            m_Beam.enabled = true;
            m_BeamTimer = 0.05f;

            target.TakeHit(damage);
            m_Cooldown = 1f / shotsPerSecond;
        }

        Enemy FindClosestEnemy()
        {
            Enemy best = null;
            var bestDistance = range;
            foreach (var enemy in Enemy.Alive)
            {
                if (enemy.IsDead) continue;
                var distance = Vector3.Distance(enemy.transform.position, transform.position);
                if (distance <= bestDistance)
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }
            return best;
        }
    }
}
