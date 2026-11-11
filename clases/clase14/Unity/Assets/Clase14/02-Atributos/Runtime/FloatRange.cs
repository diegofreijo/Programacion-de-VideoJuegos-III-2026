using System;
using UnityEngine;

namespace Clase14
{
    /// <summary>Un rango de valores [Min, Max]. Se edita con el PropertyDrawer de MinMaxRangeAttribute.</summary>
    [Serializable]
    public struct FloatRange
    {
        public float Min;
        public float Max;

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public float Average => (Min + Max) * 0.5f;
        public bool IsValid => Min <= Max;
        public float Random() => UnityEngine.Random.Range(Min, Max);

        public override string ToString() => $"{Min:0.##} a {Max:0.##}";
    }

    /// <summary>
    /// Marca un FloatRange para dibujarlo como un slider de dos puntas entre LimitMin y LimitMax.
    /// El dibujo vive en el Editor (MinMaxRangeDrawer); acá solo están los datos.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinMaxRangeAttribute : PropertyAttribute
    {
        public readonly float LimitMin;
        public readonly float LimitMax;

        public MinMaxRangeAttribute(float limitMin, float limitMax)
        {
            LimitMin = limitMin;
            LimitMax = limitMax;
        }
    }
}
