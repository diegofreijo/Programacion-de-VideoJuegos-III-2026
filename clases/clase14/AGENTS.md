# AGENTS.md — clase14

Instrucciones para agentes de código (Claude Code, Codex, Copilot, Cursor…) que trabajen en este
proyecto. Es un ejemplo para la materia: copiá la idea a tus TPs y adaptala.

## Qué es

Juego 2D top-down de oleadas de enemigos en **Unity 6000.6.5f1** con **URP** (2D Renderer), más herramientas de Editor:
Inspectors propios, una ventana de balanceo y un editor de diálogos por nodos (Graph Toolkit).
El proyecto Unity está en `Unity/`; los scripts de línea de comandos, en `scripts/`.

## Mapa del código

| Carpeta (`Unity/Assets/Clase14/`) | Assembly | Contenido |
| --- | --- | --- |
| `Runtime/` | `Clase14.Runtime` | Datos (`EnemyDefinition`, `WaveDefinition`), reglas puras (`CombatRules`) y componentes (`Enemy`, `Turret`, `WaveSpawner`, `DefendedTarget`). Diálogo de runtime: `DialogueAsset`. |
| `Editor/` | `Clase14.Editor` | PropertyDrawer, Inspectors y `BalanceWindow` en UI Toolkit; `SampleContent` (genera datos y escenas); `BuildScript`. |
| `Editor/Dialogue/` | `Clase14.Editor` | Grafo de Graph Toolkit: nodos, validación (`DialogueValidation`) e importer a `DialogueAsset`. |
| `Tests/EditMode/`, `Tests/PlayMode/` | `Clase14.Tests.*` | Tests NUnit. |
| `Data/`, `Scenes/` | — | Assets generados por `SampleContent.Generate`. |
| `Settings/` | — | URP: pipeline asset `URP-2D` con 2D Renderer. El SRP Batcher está apagado a propósito: con él, los sprites de `SpriteFactory` salen con la textura equivocada. |

## Comandos

Desde `clases/clase14`, en PowerShell, con el proyecto **cerrado** en el Editor (dos procesos de
Unity no pueden abrir el mismo proyecto):

```powershell
./scripts/Test.ps1                       # tests de Edit Mode
./scripts/Test.ps1 -Platform PlayMode    # tests de Play Mode
./scripts/Test.ps1 -TestFilter Clase14.Tests.EditMode.CombatRulesTests
./scripts/Build.ps1                      # build de Windows en Unity/Build/Windows
./scripts/Run-Method.ps1 Clase14.Editor.SampleContent.Generate   # regenerar datos y escenas
```

Todos corren Unity en `-batchmode`, sin ventanas. La primera corrida tarda más (importa el
proyecto). Los reportes y logs quedan en `Unity/TestResults/`.

## Reglas

- **Verificá antes de decir que terminaste**: los dos `Test.ps1` tienen que pasar. Si tocaste código
  de Editor o escenas, corré también `Build.ps1`.
- **Una regla nueva de juego, un test nuevo.** Las reglas van en funciones puras (como
  `CombatRules`) para poder testearlas en Edit Mode sin escena.
- **Las validaciones de datos viven en `Validate()`** de cada ScriptableObject. El Inspector, la
  ventana de balanceo y los tests las leen de ahí: no dupliques la regla en la UI.
- `OnValidate` corrige datos (clamp, valores por defecto); no avisa. Los avisos van en `Validate()`.
- La UI de Editor se hace en **UI Toolkit** (`CreateInspectorGUI`, `CreateGUI`), no en IMGUI.
- Código de Editor solo en `Editor/` (assembly `Clase14.Editor`). `Runtime/` no puede usar
  `UnityEditor` ni `Unity.GraphToolkit.Editor`, o el build falla.
- Nombres de código en inglés; textos que ve el usuario y comentarios, en español.
- Los nombres de puertos de los nodos de diálogo (`DialoguePorts`) son claves que usa el importer:
  no los renombres sin migrar los `.dialogue`.
- No edites a mano los `.asset`, `.unity` ni `.dialogue` (YAML con GUIDs). Cambiá
  `SampleContent` y regenerá, o pedile a una persona que lo haga en el Editor.
- No agregues paquetes ni cambies la versión de Unity sin preguntar.
- Si una API de Unity no te consta, buscala en la documentación de 6000.6 antes de usarla: Graph
  Toolkit cambió mucho entre versiones.

## Al entregar

Resumí qué cambiaste y por qué, qué tests agregaste y el resultado de cada script que corriste.
Commits chicos, un tema por commit.
