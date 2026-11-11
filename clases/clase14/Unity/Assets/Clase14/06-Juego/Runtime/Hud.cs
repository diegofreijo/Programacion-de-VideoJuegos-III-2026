using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Muestra arriba a la izquierda la oleada actual, los enemigos vivos y la vida del objetivo.
    /// Solo reescribe el texto cuando algo cambió: armar un string por frame genera basura (ver Profiler).
    /// </summary>
    public class Hud : MonoBehaviour
    {
        public WaveSpawner spawner;
        public DefendedTarget target;

        TextMesh m_Text;
        int m_Wave = int.MinValue, m_Alive = -1, m_Health = -1;

        void Start()
        {
            var parent = Camera.main != null ? Camera.main.transform : transform;
            m_Text = WorldText.Create(parent, "Texto del HUD", Vector3.zero, 0.6f, Color.white);
        }

        // Arriba a la izquierda de lo que ve la cámara, aunque cambie el tamaño de la ventana.
        void LateUpdate()
        {
            var camera = Camera.main;
            if (m_Text == null || camera == null) return;
            var size = camera.orthographicSize;
            m_Text.transform.localPosition = new Vector3(-size * camera.aspect + 0.4f, size - 0.4f, 10f);
        }

        void Update()
        {
            if (m_Text == null || spawner == null || target == null) return;
            var wave = spawner.CurrentWaveIndex;
            var alive = Enemy.Alive.Count;
            var health = Mathf.CeilToInt(target.Health);
            if (wave == m_Wave && alive == m_Alive && health == m_Health) return;

            m_Wave = wave;
            m_Alive = alive;
            m_Health = health;
            var waveText = wave < 0 ? "-" : $"{wave + 1}/{spawner.waves.Count}";
            m_Text.text = target.IsDestroyed
                ? "¡El objetivo cayó!"
                : $"Oleada {waveText}\nEnemigos vivos: {alive}\nObjetivo: {health}/{target.maxHealth:0}";
        }
    }
}
