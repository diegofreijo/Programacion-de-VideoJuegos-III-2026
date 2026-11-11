using System.Collections.Generic;
using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Datos de un tipo de enemigo. Los diseñadores los editan como assets, sin tocar código.
    /// </summary>
    [CreateAssetMenu(menuName = "Clase14/Enemigo", fileName = "NuevoEnemigo", order = 0)]
    public class EnemyDefinition : ScriptableObject
    {
        /// <summary>Límite de diseño: ningún enemigo debería superar este DPS.</summary>
        public const float MaxDesignDps = 20f;

        [Header("Identidad")]
        [Tooltip("Nombre que ve el jugador. Si queda vacío, OnValidate usa el nombre del asset.")]
        [Delayed] public string displayName;

        public Color color = Color.red;

        [Header("Supervivencia")]
        [Tooltip("Puntos de vida al aparecer. OnValidate los redondea a un entero.")]
        [Min(1)] public float maxHealth = 10f;

        [Tooltip("Unidades por segundo.")]
        [Range(0.1f, 10f)] public float moveSpeed = 2f;

        [Header("Ataque")]
        [Tooltip("Daño de cada golpe al objetivo.")]
        [Min(0)] public float damage = 1f;

        [Tooltip("Golpes por segundo cuando llega al objetivo.")]
        [Range(0.1f, 5f)] public float attacksPerSecond = 1f;

        [Header("Recompensa")]
        [Min(0)] public int reward = 1;

        /// <summary>Daño por segundo estimado contra el objetivo.</summary>
        public float EstimatedDps => damage * attacksPerSecond;

        /// <summary>
        /// Reglas de diseño que un dato puede romper aunque compile. Las usan el Inspector, la
        /// ventana de balanceo y los tests: una sola fuente de verdad.
        /// </summary>
        public List<string> Validate()
        {
            var problems = new List<string>();
            if (string.IsNullOrWhiteSpace(displayName))
                problems.Add("Falta el nombre visible.");
            if (damage <= 0f)
                problems.Add("El daño es 0: este enemigo nunca lastima al objetivo.");
            if (EstimatedDps > MaxDesignDps)
                problems.Add($"DPS estimado {EstimatedDps:0.#} supera el máximo de diseño ({MaxDesignDps}).");
            if (moveSpeed > 6f && maxHealth > 50f)
                problems.Add("Rápido y con mucha vida a la vez: difícil de contrarrestar.");
            if (color.a < 0.2f)
                problems.Add("El color es casi transparente: no se va a ver.");
            return problems;
        }

        public bool IsValid => Validate().Count == 0;

        // OnValidate corre en el Editor cada vez que cambia un valor en el Inspector (y al cargar).
        // Sirve para corregir datos, no para avisar: los avisos van en Validate().
        void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = name;
            maxHealth = Mathf.Max(1f, Mathf.Round(maxHealth));
        }

        // Aparece en el menú de los tres puntos del Inspector.
        [ContextMenu("Restablecer valores de diseño")]
        void ResetToDesignDefaults()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Restablecer valores de diseño");
#endif
            maxHealth = 10f;
            moveSpeed = 2f;
            damage = 1f;
            attacksPerSecond = 1f;
            reward = 1;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
