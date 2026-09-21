# Clase07: migrar las UI de demo de código a Inspector, con una escena por implementación

## Contexto y problema

Las 10 escenas de demo de `clases/clase07/Unity` (FSM, Dependency Injection, Message
Broker, MVC/MVP/MVVM) construyen su UI en runtime, en código, vía
`Clase07.Shared.UI.DemoUiFactory` (`Unity/Assets/Shared/UI/DemoUiFactory.cs`). Los
archivos `.unity` en sí son casi vacíos: uno o dos `GameObject` con un único
`MonoBehaviour`, que arma Canvas/Botones/Texto a mano por código en `Awake()`/`Start()`,
con `anchoredPosition` calculado a mano.

Esto tiene dos problemas para una clase que busca enseñar los patrones de diseño:

1. **Pedagógicamente confuso**: un estudiante que abre la escena en el Editor no ve nada
   armado — todo aparece recién al entrar en Play mode, generado por código que hay que
   leer aparte para entender qué UI existe.
2. **Se ve mal**: posiciones absolutas por pixel, sin `Layout Group`, con
   `UnityEngine.UI.Text` legacy.

Además, `01_FSM_Demo.unity` mezcla en una sola escena dos demos conceptualmente
distintos (el arma chica, comparando Baseline vs. State Pattern lado a lado, y el game
flow grande con sub-máquinas anidadas), rompiendo la convención de "una implementación,
una escena" que sí siguen los otros tres módulos.

## Objetivo

- Reemplazar `DemoUiFactory` y el patrón "UI armada en código" por jerarquías de UI
  reales, construidas como lo haría un humano en el Inspector (Canvas → Panel → Botón/
  Texto, con referencias serializadas y `Button.onClick` cableado como listener
  persistente), usando TextMeshPro y `Layout Group`/anchoring en vez de posiciones
  absolutas.
- Separar completamente cada demo en su propia escena, incluyendo partir
  `01_FSM_Demo.unity` en tres escenas independientes (Weapon Baseline, Weapon
  State Pattern, Game Flow) — se pierde la comparación lado a lado del arma a cambio de
  compartimentalización total y consistencia con el resto de los módulos.
- No tocar la lógica de dominio de ningún patrón (FSM, DI, Message Broker, MVx): el
  cambio es exclusivamente de cómo la vista obtiene y expone sus referencias de UI.

## Fuera de alcance

- No se agregan tests de PlayMode nuevos para DI/MessageBroker/MVx (hoy no los tienen;
  no lo pide el pedido original).
- No se cambia el árbol de código de dominio (`Core/`, `Small/`, `Large/`, `Shared/` de
  cada módulo) más allá de lo estrictamente necesario para que las vistas usen
  referencias serializadas.
- No se agrega arte, sprites ni animaciones — solo UI funcional prolija (paneles,
  texto, botones), consistente con el espíritu "mínimo pero legible" que ya declara el
  README del módulo.

## Inventario de escenas (antes → después)

| Módulo | Antes | Después |
|---|---|---|
| 01_FSM | `01_FSM_Demo.unity` (Weapon Baseline + StatePattern + GameFlow, todo junto) | `01_FSM_WeaponBaseline.unity`, `02_FSM_WeaponStatePattern.unity`, `03_FSM_GameFlow.unity` |
| 02_DependencyInjection | `01_DI_Singleton.unity`, `02_DI_ServiceLocator.unity`, `03_DI_VContainer.unity` | mismos nombres/rutas, reconstruidas |
| 03_MessageBroker | `01_MessageBroker_DIBroker.unity`, `02_MessageBroker_SOChannels.unity`, `03_MessageBroker_MessagePipe.unity` | mismos nombres/rutas, reconstruidas |
| 04_MVC_MVP_MVVM | `01_Mvx_MVC.unity`, `02_Mvx_MVP.unity`, `03_Mvx_MVVM.unity` | mismos nombres/rutas, reconstruidas |

Total: 10 escenas → **12 escenas**. Las de DI/MessageBroker/MVx mantienen su ruta y
nombre de archivo (se reconstruye su contenido, no su ubicación). Las nuevas escenas de
FSM viven en `Unity/Assets/01_FSM/`.

Se actualiza `EditorBuildSettings` (lista de escenas del build) para reflejar el
inventario final de 12 escenas, en el mismo orden numérico que sus nombres de archivo.

## Mecanismo de construcción

El Editor de Unity 6000.3.21f1 de este proyecto está abierto y conectado vía el paquete
`com.unity.pipeline` (ya instalado en `Packages/manifest.json` en esta sesión). Cada
escena se construye manejando ese Editor en vivo a través de `unity-cli`
(`create_scene`, `create_gameobject(s)`, `add_component`, `attach_script`, y el cableo
de `Button.onClick` como listener persistente vía la API de eventos persistentes de
Unity — el mismo resultado serializado que arrastrar una referencia en el Inspector y
elegir la función del dropdown).

Reglas del mecanismo:

- **Nunca se edita el YAML de `.unity` a mano** mientras el Editor esté conectado
  (regla dura de la skill `unity-cli`): toda modificación de escena pasa por comandos
  del Editor en vivo.
- Antes de crear cualquier texto, se importan los **TMP Essential Resources** una sola
  vez, de forma no interactiva (`TMP_PackageResourceImporter.ImportResources()`), no el
  ítem de menú modal.
- Cada escena se verifica entrando en Play mode y capturando el Game View
  (`capture_game_view`) antes de guardarla, para confirmar visualmente que se ve bien y
  que los botones responden.
- Una vez generadas y verificadas las 12 escenas, se **borran** las herramientas que
  quedan obsoletas (ver próxima sección) — las escenas `.unity` pasan a ser la única
  fuente de verdad, editable a mano en el Inspector de ahí en más.

## Qué se borra

- `Unity/Assets/Shared/UI/DemoUiFactory.cs` y su test
  `Unity/Assets/Shared/Tests/DemoUiFactoryTests.cs`.
- Los cuatro scaffolders de Editor, cuyo propósito explícito (evitar armar jerarquías a
  mano en el Editor) es lo opuesto de este cambio:
  - `Unity/Assets/Shared/Editor/SceneScaffolding.cs`
  - `Unity/Assets/01_FSM/Editor/FsmSceneScaffolding.cs`
  - `Unity/Assets/02_DependencyInjection/Editor/DiSceneScaffolding.cs`
  - `Unity/Assets/03_MessageBroker/Editor/MessageBrokerSceneScaffolding.cs`
  - `Unity/Assets/04_MVC_MVP_MVVM/Editor/MvxSceneScaffolding.cs`
- `Unity/Assets/01_FSM/Demo/FsmWeaponDemoView.cs` (se reemplaza por dos vistas, ver
  abajo).

Si borrar toda una carpeta `Editor/` de un módulo la deja vacía, se borra la carpeta
(y su `.meta`) también.

## Patrón de código nuevo por vista

Cada `MonoBehaviour` de demo dejar de construir su UI en `Awake()`/`Start()`. En su
lugar:

- Expone `[SerializeField]` privados para sus referencias de UI (`Button`,
  `TMP_Text`, `TMP_InputField`, etc.), cableadas en la escena.
- Expone un método público sin parámetros por cada acción de UI (ej.
  `OnCoinClicked()`, `OnAddClicked()`), que el `Button.onClick`/`InputField.onSubmit`
  invoca como listener persistente.
- Conserva intacta toda la lógica de dominio existente (Singleton/ServiceLocator/
  VContainer, brokers, MVC/MVP/MVVM) — el único cambio de código es reemplazar
  "construir UI y cablear en código" por "leer referencias ya cableadas".

### Caso FSM — Weapon

`FsmWeaponDemoView.cs` (arma Baseline + StatePattern juntas) se borra y se reemplaza
por dos vistas nuevas, una por escena:

- `FsmWeaponBaselineDemoView.cs`: un botón "Fire" + una etiqueta de estado, sobre
  `WeaponBaseline`.
- `FsmWeaponStatePatternDemoView.cs`: un botón "Fire" + una etiqueta de estado, sobre
  `WeaponStatePatternController`.

`FsmGameFlowDemoView.cs` se mantiene (misma lógica, 6 botones + 1 etiqueta), solo
cambia de construir su UI en código a `[SerializeField]` sobre los 6 botones y la
etiqueta.

### Caso listas dinámicas (MVC/MVP/MVVM)

Se agrega un prefab `InventoryItemRowView` (un `TMP_Text` para "Nombre x Cantidad" + un
`Button` "Remove", armado en el Inspector, con un componente `InventoryItemRowView.cs`
que expone `[SerializeField] TMP_Text nameLabel` y `[SerializeField] Button
removeButton`). Cada vista (`InventoryController`, `InventoryMvpView`,
`InventoryMvvmView`) pasa a tener `[SerializeField]` para `TMP_InputField nameInput`,
`Button addButton`, `Transform listContent` y `InventoryItemRowView rowPrefab`; el
método que hoy reconstruye la lista sigue haciendo
`Instantiate(rowPrefab, listContent)` por ítem y cableando su `removeButton.onClick`
por código.

Esto **no** reintroduce el problema original: la jerarquía estática (canvas, input,
botón "Add", contenedor de la lista) queda 100% armada en el Inspector; solo las filas
dinámicas de datos en runtime se instancian desde un prefab autoreado a mano, que es el
patrón estándar de Unity para listas de tamaño variable.

## Convenciones visuales

- **TextMeshPro** para todo texto nuevo (reemplaza `UnityEngine.UI.Text`/`InputField`
  legacy).
- `Canvas` en `Screen Space - Overlay`, `CanvasScaler` en modo "Scale With Screen
  Size".
- `Vertical`/`Horizontal Layout Group` para acomodar botones/etiquetas en vez de
  `anchoredPosition` calculado a mano.
- Un panel de fondo por escena y un título (nombre de la escena/patrón) en la parte
  superior, para que quede claro qué se está mirando al entrar en Play mode.
- Estilo de color simple y consistente entre las 12 escenas (no es un trabajo de arte,
  pero sí uniforme).

## Tests

- Se borra `DemoUiFactoryTests.cs` (la clase que testea desaparece).
- `Unity/Assets/01_FSM/Tests/PlayMode/FsmDemoViewsPlayModeTests.cs` pasa a cubrir las
  tres escenas nuevas de FSM (un test por escena, cargando cada una por su ruta,
  invocando cada botón, y verificando que las etiquetas se actualizan) en vez del único
  test actual sobre `01_FSM_Demo.unity`.
- El resto de la suite EditMode (lógica pura de dominio: `StateMachineTests`,
  `WeaponBaselineTests`, `GameFlowControllerTests`, tests de DI/MessageBroker/MVx, etc.)
  no cambia — no dependen de cómo está armada la UI.
- Verificación final: correr `EditMode` y `PlayMode` vía la invocación de batchmode que
  ya documenta el `README.md` del módulo (o el equivalente `unity test` de la CLI), y
  confirmar 0 failures.

## Documentación a actualizar

- `clases/clase07/README.md`: sección "Cómo correr cada módulo" (rutas de escena
  nuevas de FSM, y que el arma ya no se compara lado a lado sino en dos escenas
  separadas) y la mención de "UI de uGUI... sin arte" para reflejar TextMeshPro +
  Layout Groups + Inspector-first.
- `clases/clase07/SPEC.md`: línea 129 (la escena `01_FSM_Demo.unity` con "ambas
  versiones montadas...") y línea 81-83 (regla de cuándo van en escenas separadas) para
  reflejar el nuevo inventario de escenas.

## Riesgos / notas de migración

- Importar TMP Essential Resources es un paso de una sola vez a nivel proyecto; si ya
  están importados al momento de implementar, se omite sin error.
- `EditorBuildSettings.scenes` hoy se pisa por escena en cada scaffolder viejo
  (agregando una entrada por vez); al reconstruir todo de cero conviene fijar la lista
  completa de una sola vez al final, en el orden del inventario de escenas.
