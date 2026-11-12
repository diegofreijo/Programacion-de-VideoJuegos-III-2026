using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Clase14.Editor.TurretCycle
{
    /// <summary>Nombres de los puertos: son las claves con las que el importer recorre el grafo.</summary>
    public static class TurretCyclePorts
    {
        public const string In = "In";
        public const string Next = "Next";
        public const string Name = "Name";
        public const string CanFire = "CanFire";
        public const string FireRate = "FireRate";
        public const string Color = "Color";
        public const string TransitionCount = "TransitionCount";

        public static string Condition(int i) => $"Condition{i}";
        public static string Value(int i) => $"Value{i}";
        public static string TransitionOut(int i) => $"Out{i}";

        // Puertos de flujo: sin tipo de dato, con flecha. Una salida lleva a un único Estado; a una
        // entrada pueden llegar varias (ahí convergen los caminos).
        public static void AddFlowIn(Node.IPortDefinitionContext context) =>
            context.AddInputPort(In).WithDisplayName("Entrada").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

        public static void AddFlowOut(Node.IPortDefinitionContext context, string name, string displayName) =>
            context.AddOutputPort(name).WithDisplayName(displayName).WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithCapacity(PortCapacity.Single).Build();
    }

    /// <summary>Marca el Estado en el que arranca la torreta. Tiene que haber exactamente uno.</summary>
    [Serializable]
    [UseWithGraph(typeof(TurretCycleGraph))]
    [Node("Ciclo de torreta", null, "Inicio")]
    public class CycleStartNode : Node
    {
        public override void OnEnable() => DefaultColor = new Color(0.3f, 0.75f, 0.4f);

        protected override void OnDefinePorts(IPortDefinitionContext context) =>
            TurretCyclePorts.AddFlowOut(context, TurretCyclePorts.Next, "Arranca en");
    }

    /// <summary>
    /// Un Estado de la torreta. Cada transición es una salida, como las opciones del ChoiceNode del
    /// diálogo: su condición, su valor y el Estado al que lleva.
    /// </summary>
    [Serializable]
    [UseWithGraph(typeof(TurretCycleGraph))]
    [Node("Ciclo de torreta", null, "Estado")]
    public class StateNode : Node
    {
        public const int MaxTransitions = 4;

        protected override void OnDefineOptions(IOptionDefinitionContext context) =>
            context.AddOption<int>(TurretCyclePorts.TransitionCount).WithDisplayName("Transiciones").WithDefaultValue(1).Delayed();

        public int TransitionCount
        {
            get
            {
                var option = GetNodeOptionByName(TurretCyclePorts.TransitionCount);
                return option != null && option.TryGetValue<int>(out var count) ? Mathf.Clamp(count, 1, MaxTransitions) : 1;
            }
        }

        // Los puertos de transición dependen de la opción "Transiciones": se reconstruyen cuando cambia.
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            TurretCyclePorts.AddFlowIn(context);
            context.AddInputPort<string>(TurretCyclePorts.Name).WithDisplayName("Nombre").WithDefaultValue("Estado").Delayed().Build();
            context.AddInputPort<bool>(TurretCyclePorts.CanFire).WithDisplayName("Puede disparar").WithDefaultValue(true).Build();
            context.AddInputPort<float>(TurretCyclePorts.FireRate).WithDisplayName("Cadencia ×").WithDefaultValue(1f).Delayed().Build();
            context.AddInputPort<Color>(TurretCyclePorts.Color).WithDisplayName("Color").WithDefaultValue(UnityEngine.Color.white).Build();
            for (var i = 0; i < TransitionCount; i++)
            {
                context.AddInputPort<TurretCondition>(TurretCyclePorts.Condition(i)).WithDisplayName($"Condición {i + 1}")
                    .WithDefaultValue(TurretCondition.TimeElapsed).Build();
                context.AddInputPort<float>(TurretCyclePorts.Value(i)).WithDisplayName($"Valor {i + 1}").WithDefaultValue(2f).Delayed().Build();
                TurretCyclePorts.AddFlowOut(context, TurretCyclePorts.TransitionOut(i), $"Transición {i + 1}");
            }
        }
    }
}
