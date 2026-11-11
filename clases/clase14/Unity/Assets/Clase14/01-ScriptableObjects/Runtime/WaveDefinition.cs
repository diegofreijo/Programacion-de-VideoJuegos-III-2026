using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clase14
{
    [Serializable]
    public class WaveEntry
    {
        public EnemyDefinition enemy;
        [Min(1)] public int count = 5;
    }

    /// <summary>Una oleada: qué enemigos aparecen, cuántos y cada cuánto.</summary>
    [CreateAssetMenu(menuName = "Clase14/Oleada", fileName = "NuevaOleada", order = 1)]
    public class WaveDefinition : ScriptableObject
    {
        [Tooltip("Enemigos de la oleada, en orden de aparición.")]
        public List<WaveEntry> entries = new List<WaveEntry>();

        [Tooltip("Segundos entre un enemigo y el siguiente: se sortea dentro del rango.")]
        [MinMaxRange(0f, 5f)]
        public FloatRange spawnInterval = new FloatRange(0.5f, 1.5f);

        [Tooltip("Distancia al centro donde aparecen los enemigos.")]
        [Range(2f, 20f)] public float spawnRadius = 8f;

        public int TotalEnemies
        {
            get
            {
                var total = 0;
                foreach (var entry in entries)
                    if (entry != null && entry.enemy != null)
                        total += entry.count;
                return total;
            }
        }

        /// <summary>Segundos que tarda en aparecer el último enemigo, en promedio.</summary>
        public float EstimatedSpawnDuration => Mathf.Max(0, TotalEnemies - 1) * spawnInterval.Average;

        public float TotalHealth
        {
            get
            {
                var total = 0f;
                foreach (var entry in entries)
                    if (entry != null && entry.enemy != null)
                        total += entry.enemy.maxHealth * entry.count;
                return total;
            }
        }

        /// <summary>DPS que recibe el objetivo si todos los enemigos llegan a la vez.</summary>
        public float TotalDps
        {
            get
            {
                var total = 0f;
                foreach (var entry in entries)
                    if (entry != null && entry.enemy != null)
                        total += entry.enemy.EstimatedDps * entry.count;
                return total;
            }
        }

        /// <summary>El orden en que aparecen los enemigos.</summary>
        public IEnumerable<EnemyDefinition> SpawnOrder()
        {
            foreach (var entry in entries)
            {
                if (entry == null || entry.enemy == null) continue;
                for (var i = 0; i < entry.count; i++)
                    yield return entry.enemy;
            }
        }

        public List<string> Validate()
        {
            var problems = new List<string>();
            if (entries.Count == 0)
                problems.Add("La oleada no tiene enemigos.");
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.enemy == null)
                    problems.Add($"Fila {i + 1}: falta asignar el enemigo.");
                else if (!entry.enemy.IsValid)
                    problems.Add($"Fila {i + 1}: {entry.enemy.name} tiene datos inválidos.");
            }
            if (!spawnInterval.IsValid)
                problems.Add("El intervalo mínimo es mayor que el máximo.");
            return problems;
        }

        public bool IsValid => Validate().Count == 0;

        void OnValidate()
        {
            // Corrige en vez de avisar: un rango invertido se da vuelta solo.
            if (!spawnInterval.IsValid)
                spawnInterval = new FloatRange(spawnInterval.Max, spawnInterval.Min);
            foreach (var entry in entries)
                if (entry != null && entry.count < 1)
                    entry.count = 1;
        }
    }
}
