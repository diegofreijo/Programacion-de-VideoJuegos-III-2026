using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Clase14
{
    /// <summary>Hace aparecer las oleadas una tras otra alrededor del objetivo.</summary>
    public class WaveSpawner : MonoBehaviour
    {
        public DefendedTarget target;
        public List<WaveDefinition> waves = new List<WaveDefinition>();

        [Tooltip("Segundos de descanso entre oleadas.")]
        [Min(0)] public float pauseBetweenWaves = 3f;

        [Tooltip("Los enemigos buscan el objetivo cada frame generando basura. Para mirar en el Profiler.")]
        public bool carelessEnemies;

        [Tooltip("Si está activo, arranca solo al entrar en Play Mode.")]
        public bool playOnStart = true;

        public int CurrentWaveIndex { get; private set; } = -1;
        public int SpawnedCount { get; private set; }

        void Start()
        {
            if (playOnStart)
                StartCoroutine(PlayAll());
        }

        public IEnumerator PlayAll()
        {
            for (var i = 0; i < waves.Count; i++)
            {
                CurrentWaveIndex = i;
                yield return PlayWave(waves[i]);
                yield return new WaitForSeconds(pauseBetweenWaves);
            }
        }

        public IEnumerator PlayWave(WaveDefinition wave)
        {
            foreach (var definition in wave.SpawnOrder())
            {
                Spawn(definition, wave.spawnRadius);
                yield return new WaitForSeconds(wave.spawnInterval.Random());
            }
        }

        public Enemy Spawn(EnemyDefinition definition, float radius)
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var center = target != null ? target.transform.position : transform.position;
            var position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            var enemy = new GameObject().AddComponent<Enemy>();
            enemy.transform.position = position;
            enemy.Init(definition, target, carelessEnemies);
            SpawnedCount++;
            return enemy;
        }
    }
}
