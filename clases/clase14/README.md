# Clase 14 — Herramientas en Unity y agentes de LLM

Proyecto de demostración para la clase: un juego 2D top-down de **oleadas de enemigos** cuyos
datos viven en ScriptableObjects, y un conjunto de **herramientas de Editor** para que un
diseñador los edite sin depender del programador. Unity **6000.6.5f1** (Graph Toolkit necesita
6000.6 o posterior).

Abrí `clases/clase14/Unity` desde Unity Hub. Hay cuatro escenas (menú **Clase14 > Escenas** para
saltar entre ellas):

- **Main**: al darle Play aparecen dos oleadas que caminan hacia el objetivo del centro y una
  torreta las dispara. Arriba a la izquierda, un HUD con la oleada, los enemigos vivos y la vida del
  objetivo. En la consola se imprime el diálogo de ejemplo.
- **Dialogo**: el diálogo del editor de nodos usado en un juego. Mové al jugador con WASD o las
  flechas hasta el Guardia, apretá **E** para hablar, **1**/**2** para elegir y **Espacio** para seguir.
- **Profiler**: una oleada gigante escrita mal a propósito, para ver picos de GC (ver abajo).
- **Torreta**: una torreta manejada por un ciclo de estados hecho con Graph Toolkit. Se pone roja
  cuando se recalienta, naranja mientras se enfría y vuelve a amarilla, contra una oleada larga.

## Recorrido de la clase

El código está en `Unity/Assets/Clase14/`, en una carpeta por tema, numeradas en el orden de la
clase. Cada una tiene su `Runtime/` (código del juego) y/o su `Editor/` (solo corre en el Editor),
un assembly propio (`.asmdef`) y un `LEEME.md` que dice qué mirar primero.

| # | Tema | Dónde |
| --- | --- | --- |
| 01 | Datos en ScriptableObjects; atributos `[Header]`, `[Tooltip]`, `[Range]`, `[Min]`, `[ContextMenu]`, `[CreateAssetMenu]`; `OnValidate` corrigiendo datos; `Validate()` con los avisos | `01-ScriptableObjects/Runtime/EnemyDefinition.cs`, `WaveDefinition.cs`; assets en `Data/` |
| 02 | Atributo propio y PropertyDrawer (rango min–max), en UI Toolkit **y** en IMGUI | `02-Atributos/Runtime/FloatRange.cs`, `02-Atributos/Editor/MinMaxRangeDrawer.cs` |
| 03 | Custom Inspector en UI Toolkit con vista previa, validaciones y botones | `03-Inspectors/Editor/EnemyDefinitionEditor.cs`, `WaveDefinitionEditor.cs` |
| 04 | EditorWindow de balanceo (tabla editable, datos inválidos en rojo, `MenuItem`) | `04-Ventanas/Editor/BalanceWindow.cs` — menú **Clase14 > Ventana de balanceo** (Ctrl+Alt+B) |
| 05 | Editor de diálogos por nodos con Graph Toolkit, el importer que lo convierte en un asset de runtime y el juego que lo usa | `05-GraphToolkit/Editor/Dialogue/` — doble clic en `Data/Dialogos/Guardia.dialogue`; `05-GraphToolkit/Runtime/DialogueNpc.cs` y la escena **Dialogo** |
| 06 | El juego: componentes, reglas puras (`CombatRules`), HUD; el código con basura para el Profiler | `06-Juego/Runtime/` |
| 07 | Tests de Edit Mode y Play Mode | `07-Tests/EditMode/`, `07-Tests/PlayMode/` |
| 08 | Build por línea de comandos, generación del contenido de ejemplo y capturas en batch | `08-Build/Editor/BuildScript.cs`, `SampleContent.cs`, `SceneCapture.cs`, `scripts/Build.ps1` |
| 09 | Un grafo que necesita ser grafo: el ciclo de sobrecalentamiento de la torreta (tiene un ciclo y caminos que convergen), con el mismo modelo grafo → importer → asset | `09-CicloTorreta/` — doble clic en `Data/CiclosTorreta/Sobrecalentamiento.turretcycle`; escena **Torreta** |
| — | Instrucciones para agentes de código | [`AGENTS.md`](AGENTS.md) |

Hay dos enemigos con datos inválidos a propósito (**Berserker**: DPS por encima del máximo de
diseño; **Fantasma**: color casi transparente) y una oleada que los usa (`Oleada3Jefe`), para ver
cómo se marcan los problemas en el Inspector y en la ventana de balanceo.

### Assemblies

Un `.asmdef` por tema y por lado: `Clase14.ScriptableObjects`, `Clase14.Atributos`,
`Clase14.Atributos.Editor`, `Clase14.Inspectors.Editor`, `Clase14.Ventanas.Editor`,
`Clase14.GraphToolkit`, `Clase14.GraphToolkit.Editor`, `Clase14.Juego`, `Clase14.Build.Editor`,
`Clase14.CicloTorreta`, `Clase14.CicloTorreta.Editor` y los dos de tests. Cada uno declara de cuáles depende, y Unity no permite ciclos: por eso el
Inspector de oleadas abre la ventana de balanceo por su menú (`EditorApplication.ExecuteMenuItem`)
en vez de llamar a `BalanceWindow`. Los assemblies `*.Editor` solo se compilan en el Editor y no
entran en el build.

Ojo al renombrar o mover un assembly: los `.dialogue` guardan el nombre del assembly de cada nodo.
Los nodos tienen `[MovedFrom]` para que los grafos guardados antes sigan abriendo.

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

El segundo grafo, el ciclo de la torreta (`09-CicloTorreta/`), usa el mismo modelo con datos que no
entran en una lista: un ciclo de estados con caminos que convergen. Cada grafo declara sus nodos
con `[UseWithGraph]` para que no aparezcan en el otro.

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

Para que VS Code compile y dé IntelliSense, Unity tiene que generar la solución (`Unity/Unity.slnx`
y los `.csproj`, que no se versionan). Eso lo hace el paquete `com.unity.ide.visualstudio`: en
**Edit > Preferences > External Tools** elegir *Visual Studio Code* y tocar **Regenerate project
files**. VS Code necesita las extensiones *C# Dev Kit* y *Unity*; abriendo `clases/clase14` carga
esa solución sola.

`scripts/Run-Method.ps1` ejecuta cualquier método estático de Editor en batch. Por ejemplo,
`./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.Generate` regenera los enemigos, oleadas,
el diálogo, el ciclo de la torreta y las escenas de ejemplo; `Clase14.Editor.SampleContent.GenerateScenes`,
solo las escenas. Con `-Graphics` corre con dispositivo gráfico (sin ventanas igual), para métodos
que renderizan: `./scripts/Run-Method.ps1 -Graphics Clase14.Editor.SceneCapture.CaptureOverheatedTurret`
deja en `Unity/TestResults/torreta-recalentada.png` la escena Torreta con la torreta recalentada.

## Profiler

Abrir `Scenes/Profiler.unity` (**Clase14 > Escenas > Profiler**), abrir **Window > Analysis > Profiler** y darle Play. La oleada
gigante (600 enemigos) usa `carelessEnemies`: cada enemigo busca el objetivo en cada frame con
`FindObjectsByType`, LINQ y concatenación de strings (`Enemy.FindTargetCarelessly`). En el módulo
**CPU Usage** se ven los picos de **GC.Alloc** y de recolección de basura. Desactivar
`carelessEnemies` en el objeto *Oleadas* y comparar.
