# Clase 14 — Herramientas en Unity y agentes de LLM

Proyecto de demostración para la clase: un juego 2D top-down de **oleadas de enemigos** cuyos
datos viven en ScriptableObjects, y un conjunto de **herramientas de Editor** para que un
diseñador los edite sin depender del programador. Unity **6000.6.5f1** (Graph Toolkit necesita
6000.6 o posterior).

Abrí `clases/clase14/Unity` desde Unity Hub y la escena `Assets/Clase14/Scenes/Main.unity`.
Al darle Play aparecen dos oleadas que caminan hacia el objetivo del centro y una torreta las
dispara. En la consola se imprime el diálogo de ejemplo.

## Qué mirar

| Tema | Dónde |
| --- | --- |
| Datos en ScriptableObjects | `Runtime/EnemyDefinition.cs`, `Runtime/WaveDefinition.cs`; assets en `Data/` |
| Atributos: `[Header]`, `[Tooltip]`, `[Range]`, `[Min]`, `[ContextMenu]`, `[CreateAssetMenu]` | `EnemyDefinition.cs` |
| `OnValidate` corrigiendo datos | `EnemyDefinition.OnValidate`, `WaveDefinition.OnValidate` |
| PropertyDrawer propio (rango min–max), en UI Toolkit **y** en IMGUI | `Editor/MinMaxRangeDrawer.cs` |
| Custom Inspector en UI Toolkit con vista previa, validaciones y botones | `Editor/EnemyDefinitionEditor.cs`, `Editor/WaveDefinitionEditor.cs` |
| EditorWindow de balanceo (tabla editable, datos inválidos en rojo, `MenuItem`) | `Editor/BalanceWindow.cs` — menú **Clase14 > Ventana de balanceo** (Ctrl+Alt+B) |
| Editor de diálogos por nodos con Graph Toolkit | `Editor/Dialogue/` — doble clic en `Data/Dialogos/Guardia.dialogue` |
| Importer: del grafo a un asset de runtime | `Editor/Dialogue/DialogueImporter.cs` → `Runtime/DialogueAsset.cs` |
| Tests de Edit Mode y Play Mode | `Tests/EditMode/`, `Tests/PlayMode/` |
| Build por línea de comandos | `Editor/BuildScript.cs`, `scripts/Build.ps1` |
| Picos de GC para el Profiler | escena `Scenes/Profiler.unity` |
| Instrucciones para agentes de código | [`AGENTS.md`](AGENTS.md) |

(Las rutas de la tabla son relativas a `Unity/Assets/Clase14/`.)

Hay dos enemigos con datos inválidos a propósito (**Berserker**: DPS por encima del máximo de
diseño; **Fantasma**: color casi transparente) y una oleada que los usa (`Oleada3Jefe`), para ver
cómo se marcan los problemas en el Inspector y en la ventana de balanceo.

## IMGUI y UI Toolkit

Unity tiene dos sistemas de UI para el Editor. **IMGUI** (`OnGUI`, `OnInspectorGUI`,
`EditorGUILayout`) redibuja todo en cada frame y es lo que aparece en la mayoría de los tutoriales
viejos. **UI Toolkit** (`CreateInspectorGUI`, `CreatePropertyGUI`, `CreateGUI`) arma un árbol de
elementos una vez y lo mantiene sincronizado con los datos mediante *bindings*. Es el sistema
recomendado hoy y el que usan todas las herramientas de este proyecto. `MinMaxRangeDrawer` tiene
las dos versiones una al lado de la otra para compararlas.

## Graph Toolkit

Graph Toolkit es el módulo de Unity para hacer editores basados en nodos. Uno declara el grafo
(`DialogueGraph`), los nodos y sus puertos (`DialogueNodes.cs`) y las reglas de validación
(`OnGraphChanged`); la ventana, el zoom, los cables y el guardado los provee Unity. Graph Toolkit
**no ejecuta** el grafo: un `ScriptedImporter` lo convierte en un asset simple
(`DialogueAsset`) que es lo único que usa el juego.

## Ejecutar tests y build sin abrir el Editor

Igual que en la clase 08, con el proyecto **cerrado** en Unity. Desde `clases/clase14`:

```powershell
./scripts/Test.ps1                       # Edit Mode
./scripts/Test.ps1 -Platform PlayMode    # Play Mode
./scripts/Build.ps1                      # Unity/Build/Windows/Clase14.exe
```

Desde VS Code, abriendo la carpeta `clases/clase14`, están las tareas equivalentes en
**Tasks: Run Task**. Los scripts buscan Unity en la carpeta por defecto de Unity Hub; si está en
otro lado, definir `UNITY_EDITOR_PATH`. Cada corrida deja su reporte NUnit y su log en
`Unity/TestResults/`.

`scripts/Run-Method.ps1` ejecuta cualquier método estático de Editor en batch. Por ejemplo,
`./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.Generate` regenera los enemigos, oleadas,
el diálogo y las escenas de ejemplo.

## Profiler

Abrir `Scenes/Profiler.unity`, abrir **Window > Analysis > Profiler** y darle Play. La oleada
gigante (600 enemigos) usa `carelessEnemies`: cada enemigo busca el objetivo en cada frame con
`FindObjectsByType`, LINQ y concatenación de strings (`Enemy.FindTargetCarelessly`). En el módulo
**CPU Usage** se ven los picos de **GC.Alloc** y de recolección de basura. Desactivar
`carelessEnemies` en el objeto *Oleadas* y comparar.
