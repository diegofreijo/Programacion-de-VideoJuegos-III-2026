using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Clase14.Editor
{
    /// <summary>
    /// Dibuja un FloatRange marcado con [MinMaxRange] como un slider de dos puntas y dos campos.
    /// Tiene las dos versiones: UI Toolkit (CreatePropertyGUI) e IMGUI (OnGUI), para compararlas.
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        // UI Toolkit: se construye una vez y los bindings mantienen los valores al día.
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var limits = (MinMaxRangeAttribute)attribute;
            var min = property.FindPropertyRelative(nameof(FloatRange.Min));
            var max = property.FindPropertyRelative(nameof(FloatRange.Max));

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };

            var slider = new MinMaxSlider(property.displayName, min.floatValue, max.floatValue, limits.LimitMin, limits.LimitMax)
            {
                tooltip = property.tooltip,
                style = { flexGrow = 1 },
            };
            slider.AddToClassList(BaseField<Vector2>.alignedFieldUssClassName);
            slider.RegisterValueChangedCallback(evt =>
            {
                min.floatValue = Round(evt.newValue.x);
                max.floatValue = Round(evt.newValue.y);
                property.serializedObject.ApplyModifiedProperties();
            });
            // Si los datos cambian por otro lado (Undo, OnValidate, la ventana de balanceo), el slider se actualiza.
            slider.TrackPropertyValue(property, _ => slider.SetValueWithoutNotify(new Vector2(min.floatValue, max.floatValue)));

            var minField = new FloatField { isDelayed = true, style = { width = 48 } };
            minField.BindProperty(min);
            var maxField = new FloatField { isDelayed = true, style = { width = 48 } };
            maxField.BindProperty(max);

            row.Add(slider);
            row.Add(minField);
            row.Add(maxField);
            return row;
        }

        // IMGUI: se ejecuta en cada repintado y hay que calcular los rectángulos a mano.
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var limits = (MinMaxRangeAttribute)attribute;
            var min = property.FindPropertyRelative(nameof(FloatRange.Min));
            var max = property.FindPropertyRelative(nameof(FloatRange.Max));

            EditorGUI.BeginProperty(position, label, property);
            var fieldsRect = EditorGUI.PrefixLabel(position, label);
            const float fieldWidth = 48f;
            var sliderRect = new Rect(fieldsRect.x, fieldsRect.y, fieldsRect.width - 2 * fieldWidth - 4, fieldsRect.height);
            var minRect = new Rect(sliderRect.xMax + 2, fieldsRect.y, fieldWidth, fieldsRect.height);
            var maxRect = new Rect(minRect.xMax + 2, fieldsRect.y, fieldWidth, fieldsRect.height);

            var minValue = min.floatValue;
            var maxValue = max.floatValue;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, limits.LimitMin, limits.LimitMax);
            minValue = EditorGUI.FloatField(minRect, minValue);
            maxValue = EditorGUI.FloatField(maxRect, maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                min.floatValue = Round(minValue);
                max.floatValue = Round(maxValue);
            }
            EditorGUI.EndProperty();
        }

        static float Round(float value) => Mathf.Round(value * 100f) / 100f;
    }
}
