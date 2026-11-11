using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Clase14.Editor.Dialogue
{
    /// <summary>Nombres de los puertos: son las claves con las que el importer recorre el grafo.</summary>
    public static class DialoguePorts
    {
        public const string In = "In";
        public const string Next = "Next";
        public const string Speaker = "Speaker";
        public const string Text = "Text";
        public const string ChoiceCount = "ChoiceCount";

        public static string ChoiceText(int i) => $"Choice{i}";
        public static string ChoiceOut(int i) => $"Out{i}";

        // Puertos de flujo: sin tipo de dato, con flecha. Una salida lleva a un único nodo.
        public static void AddFlowIn(Node.IPortDefinitionContext context) =>
            context.AddInputPort(In).WithDisplayName("Entrada").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

        public static void AddFlowOut(Node.IPortDefinitionContext context, string name, string displayName) =>
            context.AddOutputPort(name).WithDisplayName(displayName).WithConnectorUI(PortConnectorUI.Arrowhead)
                .WithCapacity(PortCapacity.Single).Build();
    }

    /// <summary>Donde empieza el diálogo. Tiene que haber exactamente uno.</summary>
    // MovedFrom: estas clases estaban en el assembly "Clase14.Editor". Los .dialogue guardan el
    // nombre del assembly de cada nodo; sin esto, al renombrarlo los nodos quedan "Missing".
    [MovedFrom(true, sourceAssembly: "Clase14.Editor")]
    [Serializable]
    [Node("Diálogo", null, "Inicio")]
    public class StartNode : Node
    {
        public override void OnEnable() => DefaultColor = new Color(0.3f, 0.75f, 0.4f);

        protected override void OnDefinePorts(IPortDefinitionContext context) =>
            DialoguePorts.AddFlowOut(context, DialoguePorts.Next, "Siguiente");
    }

    /// <summary>Una línea dicha por un personaje.</summary>
    [MovedFrom(true, sourceAssembly: "Clase14.Editor")]
    [Serializable]
    [Node("Diálogo", null, "Línea")]
    public class LineNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            DialoguePorts.AddFlowIn(context);
            context.AddInputPort<string>(DialoguePorts.Speaker).WithDisplayName("Personaje").WithDefaultValue("Guardia").Delayed().Build();
            context.AddInputPort<string>(DialoguePorts.Text).WithDisplayName("Texto").AsTextArea(2, 5).Delayed().Build();
            DialoguePorts.AddFlowOut(context, DialoguePorts.Next, "Siguiente");
        }
    }

    /// <summary>Una línea con opciones: cada opción tiene su texto y su propia salida.</summary>
    [MovedFrom(true, sourceAssembly: "Clase14.Editor")]
    [Serializable]
    [Node("Diálogo", null, "Elección")]
    public class ChoiceNode : Node
    {
        public override void OnEnable() => DefaultColor = new Color(0.95f, 0.7f, 0.2f);

        protected override void OnDefineOptions(IOptionDefinitionContext context) =>
            context.AddOption<int>(DialoguePorts.ChoiceCount).WithDisplayName("Opciones").WithDefaultValue(2).Delayed();

        public int ChoiceCount
        {
            get
            {
                var option = GetNodeOptionByName(DialoguePorts.ChoiceCount);
                return option != null && option.TryGetValue<int>(out var count) ? Mathf.Clamp(count, 1, 6) : 2;
            }
        }

        // Los puertos dependen de la opción "Opciones": se reconstruyen cuando cambia.
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            DialoguePorts.AddFlowIn(context);
            context.AddInputPort<string>(DialoguePorts.Speaker).WithDisplayName("Personaje").WithDefaultValue("Guardia").Delayed().Build();
            context.AddInputPort<string>(DialoguePorts.Text).WithDisplayName("Pregunta").AsTextArea(2, 5).Delayed().Build();
            for (var i = 0; i < ChoiceCount; i++)
            {
                context.AddInputPort<string>(DialoguePorts.ChoiceText(i)).WithDisplayName($"Opción {i + 1}").Delayed().Build();
                DialoguePorts.AddFlowOut(context, DialoguePorts.ChoiceOut(i), $"Si elige {i + 1}");
            }
        }
    }

    /// <summary>Fin explícito del diálogo.</summary>
    [MovedFrom(true, sourceAssembly: "Clase14.Editor")]
    [Serializable]
    [Node("Diálogo", null, "Fin")]
    public class EndNode : Node
    {
        public override void OnEnable() => DefaultColor = new Color(0.85f, 0.3f, 0.3f);

        protected override void OnDefinePorts(IPortDefinitionContext context) => DialoguePorts.AddFlowIn(context);
    }
}
