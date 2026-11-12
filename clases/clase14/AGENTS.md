# AGENTS.md — clase14

Instrucciones para agentes de código (Claude Code, Codex, Copilot, Cursor…) que trabajen en este
proyecto. Es un ejemplo para la materia: copiá la idea a tus TPs y adaptala.

## Qué es

Juego 2D top-down de oleadas de enemigos en **Unity 6000.6.5f1** con **URP** (2D Renderer), más herramientas de Editor:
Inspectors propios, una ventana de balanceo y dos grafos de Graph Toolkit: un editor de diálogos y
el ciclo de sobrecalentamiento de la torreta.
El proyecto Unity está en `Unity/`; los scripts de línea de comandos, en `scripts/`.

## Mapa del código

Una carpeta por tema de la clase en `Unity/Assets/Clase14/`, cada una con su `Runtime/` y/o
`Editor/` y un `.asmdef` por lado. Los namespaces no siguen las carpetas: `Clase14` (runtime),
`Clase14.Editor`, `Clase14.Editor.Dialogue` y `Clase14.Editor.TurretCycle`. Ojo: dentro de
`Clase14.Editor`, `TurretCycle` es ese namespace; el componente se escribe `Clase14.TurretCycle`.

| Carpeta | Assembly | Contenido |
| --- | --- | --- |
| `01-ScriptableObjects/Runtime/` | `Clase14.ScriptableObjects` | Datos: `EnemyDefinition`, `WaveDefinition` (con su `Validate()`). |
| `02-Atributos/Runtime/`, `Editor/` | `Clase14.Atributos`, `.Editor` | `FloatRange` + `MinMaxRangeAttribute`; su PropertyDrawer. |
| `03-Inspectors/Editor/` | `Clase14.Inspectors.Editor` | Inspectors de enemigo y oleada; `EditorUi` (piezas de UI compartidas). |
| `04-Ventanas/Editor/` | `Clase14.Ventanas.Editor` | `BalanceWindow`. |
| `05-GraphToolkit/Runtime/` | `Clase14.GraphToolkit` | `DialogueAsset` (lo que genera el importer), `DialogueNpc`, `DialogueBox`, `PlayerMover`, `DialoguePlayer`. |
| `05-GraphToolkit/Editor/Dialogue/` | `Clase14.GraphToolkit.Editor` | Grafo de Graph Toolkit: nodos, validación (`DialogueValidation`) e importer. |
| `06-Juego/Runtime/` | `Clase14.Juego` | Reglas puras (`CombatRules`) y componentes (`Enemy`, `Turret`, `WaveSpawner`, `DefendedTarget`, `Hud`); `SpriteFactory`, `WorldText`. |
| `07-Tests/EditMode/`, `PlayMode/` | `Clase14.Tests.*` | Tests NUnit. |
| `08-Build/Editor/` | `Clase14.Build.Editor` | `BuildScript`, `SampleContent` (genera datos y escenas), `SceneMenu`, `SceneCapture` (PNG en batch). |
| `09-CicloTorreta/Runtime/` | `Clase14.CicloTorreta` | `TurretCycleAsset` (lo que genera el importer, con el enum `TurretCondition`), `TurretCycleRunner` (recorrido puro), `TurretCycle` (maneja a la `Turret`). |
| `09-CicloTorreta/Editor/` | `Clase14.CicloTorreta.Editor` | Grafo `.turretcycle`: nodos Inicio y Estado (`TurretCyclePorts`), validación (`TurretCycleValidation`) e importer. |
| `Data/`, `Scenes/` | — | Assets generados por `SampleContent`. Escenas: `Main`, `Dialogo`, `Profiler`, `Torreta`. |
| `Settings/` | — | URP: pipeline asset `URP-2D` con 2D Renderer. El SRP Batcher está apagado a propósito: con él, los sprites de `SpriteFactory` salen con la textura equivocada. |

Dependencias entre assemblies (sin ciclos): `ScriptableObjects` → `Atributos`; `Juego` →
`ScriptableObjects`; `GraphToolkit` → `Juego`; `CicloTorreta` → `Juego`; los `*.Editor` → los de runtime que editan;
`Ventanas.Editor` → `Inspectors.Editor`; `Build.Editor` → todos. Si un archivo nuevo necesita un
tipo de otro tema, agregá la referencia en el `.asmdef`; si eso arma un ciclo, la dependencia está
al revés.

## Comandos

Desde `clases/clase14`, en PowerShell, con el proyecto **cerrado** en el Editor (dos procesos de
Unity no pueden abrir el mismo proyecto):

```powershell
./scripts/Test.ps1                       # tests de Edit Mode
./scripts/Test.ps1 -Platform PlayMode    # tests de Play Mode
./scripts/Test.ps1 -TestFilter Clase14.Tests.EditMode.CombatRulesTests
./scripts/Build.ps1                      # build de Windows en Unity/Build/Windows
./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.Generate         # regenerar datos y escenas
./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.GenerateScenes   # solo las escenas
./scripts/Run-Method.ps1 -Graphics Clase14.Editor.SceneCapture.CaptureOverheatedTurret  # PNG de la escena Torreta
```

Todos corren Unity en `-batchmode`, sin ventanas. La primera corrida tarda más (importa el
proyecto). Los reportes, logs y capturas quedan en `Unity/TestResults/`. Para verificar algo visual,
leé la captura: nunca abras el juego con ventana.

## Reglas

- **Verificá antes de decir que terminaste**: los dos `Test.ps1` tienen que pasar. Si tocaste código
  de Editor o escenas, corré también `Build.ps1`.
- **Una regla nueva de juego, un test nuevo.** Las reglas van en funciones puras (como
  `CombatRules`) para poder testearlas en Edit Mode sin escena.
- **Las validaciones de datos viven en `Validate()`** de cada ScriptableObject. El Inspector, la
  ventana de balanceo y los tests las leen de ahí: no dupliques la regla en la UI.
- `OnValidate` corrige datos (clamp, valores por defecto); no avisa. Los avisos van en `Validate()`.
- La UI de Editor se hace en **UI Toolkit** (`CreateInspectorGUI`, `CreateGUI`), no en IMGUI.
- Código nuevo en la carpeta del tema que muestra. Código de Editor solo en una carpeta `Editor/`
  (assemblies `*.Editor`). Las carpetas `Runtime/` no pueden usar `UnityEditor` ni
  `Unity.GraphToolkit.Editor`, o el build falla.
- Al mover o renombrar clases de nodos, o su assembly, agregá `[MovedFrom]`: los `.dialogue` guardan
  el tipo con su assembly.
- Nombres de código en inglés; textos que ve el usuario y comentarios, en español.
- Los nombres de puertos de los nodos (`DialoguePorts`, `TurretCyclePorts`) son claves que usa el
  importer: no los renombres sin migrar los `.dialogue` y `.turretcycle`.
- Cada nodo declara su grafo con `[UseWithGraph]`; uno nuevo sin eso aparece en todos los grafos.
- No edites a mano los `.asset`, `.unity`, `.dialogue` ni `.turretcycle` (YAML con GUIDs). Cambiá
  `SampleContent` y regenerá, o pedile a una persona que lo haga en el Editor.
- No agregues paquetes ni cambies la versión de Unity sin preguntar.
- Si una API de Unity no te consta, buscala en la documentación de 6000.6 antes de usarla: Graph
  Toolkit cambió mucho entre versiones.

## Al entregar

Resumí qué cambiaste y por qué, qué tests agregaste y el resultado de cada script que corriste.
Commits chicos, un tema por commit.
