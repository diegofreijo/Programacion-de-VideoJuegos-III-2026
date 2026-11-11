# 03 · Custom Inspectors

Reemplazan el Inspector de un tipo de asset por uno propio, en UI Toolkit (`CreateInspectorGUI`).

Mirá primero `Editor/EnemyDefinitionEditor.cs`: los campos de siempre (`InspectorElement`), una
vista previa calculada (DPS, segundos para matarlo), las validaciones de `Validate()` y botones.
Después `Editor/WaveDefinitionEditor.cs`. `Editor/EditorUi.cs` tiene las piezas que comparten.

Para verlo: seleccioná `Data/Enemigos/Orco` y después `Berserker` (inválido a propósito).
