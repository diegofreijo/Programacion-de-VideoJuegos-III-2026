using System;
using System.Collections.Generic;
using UnityEngine;

namespace Clase14
{
    /// <summary>Lo que tiene que pasar para salir de un Estado. Cuenta desde que se entró al Estado.</summary>
    public enum TurretCondition
    {
        [InspectorName("Disparos ≥ N")] ShotsFired,
        [InspectorName("Pasaron X s")] TimeElapsed,
        [InspectorName("Sin enemigos en alcance")] NoEnemiesInRange,
    }

    /// <summary>
    /// Versión de runtime de un ciclo de torreta. La genera el importer a partir del grafo
    /// (.turretcycle): el juego nunca ve el grafo, solo esta lista de estados enlazados por índice.
    /// </summary>
    public class TurretCycleAsset : ScriptableObject
    {
        public const int None = -1;

        [Serializable]
        public class Transition
        {
            public TurretCondition condition;
            [Tooltip("N disparos o X segundos. No se usa con «Sin enemigos en alcance».")]
            public float value;
            [Tooltip("Índice del Estado al que lleva; -1 si la salida no está conectada.")]
            public int next = None;
        }

        [Serializable]
        public class State
        {
            public string name;
            public bool canFire = true;
            public float fireRateMultiplier = 1f;
            public Color color = Color.white;
            [Tooltip("Se evalúan en orden: gana la primera que se cumple.")]
            public List<Transition> transitions = new List<Transition>();
        }

        public int start = None;
        public List<State> states = new List<State>();
    }
}
