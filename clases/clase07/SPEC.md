# Clase 07 — Patrones de diseño para juegos: FSM, DI, Message Broker, MVC/MVP/MVVM

## Objetivo

Dar a los estudiantes al menos dos implementaciones concretas y comparables de cada uno
de estos patrones, en un único proyecto de Unity real (no simulado con stubs), de forma
que puedan:

- Ver el mismo problema resuelto de más de una manera y entender el trade-off de cada
  enfoque (desde la versión más ingenua/rápida hasta la más productiva/usada en la
  industria).
- Correr cada ejemplo de forma mínima (Play mode con muy poca puesta en escena: botones,
  texto de debug, algún log) sin necesidad de arte ni pulido.
- Ver tests automatizados (NUnit / Unity Test Framework) sobre la lógica de cada
  implementación, en la misma línea que la separación dominio/infraestructura ya
  trabajada en `clase05`.

Este spec cubre las cuatro familias de patrones como módulos independientes dentro de un
mismo proyecto de Unity. Cada módulo se implementa, testea y commitea por separado, pero
todos comparten spec, plan y convenciones.

## Decisiones de alcance (por qué esto y no otra cosa)

- **Un solo proyecto Unity real** (no plain C# con `UnityStubs` como en `clase05`),
  porque VContainer y MessagePipe son paquetes de Unity y no tiene sentido simularlos, y
  porque ver los ejemplos corriendo en el Editor (Play mode) es más directo para la
  clase que un `dotnet run` de consola.
- **NUnit / Unity Test Framework en vez de xUnit**, a pesar de que `clase05` usa xUnit y
  es la preferencia general del profesor. Motivo verificado en la conversación de diseño:
  el Test Runner del Editor de Unity (EditMode/PlayMode, `-runTests` en batchmode,
  `[UnityTest]` con corutinas) es específicamente NUnit; no existe un runner de xUnit
  integrado a Unity. Como la mayoría de la lógica de estos módulos vive directamente en
  `MonoBehaviour`/`ScriptableObject` dentro del proyecto de Unity (a diferencia de
  `clase05`, que aislaba todo el dominio en una librería plain C#), xUnit no llegaría a
  poder testear casi nada. Se decidió no partir la lógica en un proyecto aparte solo para
  poder usar xUnit: todo el testing de `clase07` es NUnit/UTF.
- **Verificación sin abrir el Editor interactivamente**: el agente que implementa este
  spec no puede manejar el Editor gráficamente. La verificación se hace corriendo Unity
  en `-batchmode` (creación del proyecto, resolución de paquetes, compilación, y
  `-runTests` para EditMode/PlayMode). La revisión visual final (que se vea bien en Play
  mode) queda a cargo del usuario, abriendo el proyecto en su propio Editor.
- **Módulos independientes, un solo spec**: las cuatro familias de patrones no comparten
  estado ni dominio entre sí a propósito — cada una tiene su propio mini-dominio de
  juguete, para que se puedan leer y calificar de forma aislada. Comparten únicamente
  convenciones (estructura de carpetas, testing, namespaces).

## Estructura general del proyecto

```
clases/clase07/
  README.md                        # overview del proyecto Unity y cómo abrirlo/correrlo
  SPEC.md                          # este documento
  Unity/                           # proyecto de Unity real
    Packages/manifest.json         # incluye VContainer y MessagePipe
    Assets/
      01_FSM/
      02_DependencyInjection/
      03_MessageBroker/
      04_MVC_MVP_MVVM/
    ProjectSettings/
```

- Unity **6000.3.21f1** (versión ya instalada localmente; el proyecto se crea con
  `Unity -batchmode -createProject`).
- Cada carpeta de implementación (dentro de cada módulo, ver la convención de letras en
  "Convenciones compartidas" más abajo) tiene su propio **Assembly Definition** (asmdef),
  para mantener límites de compilación claros y namespaces separados — no hay un asmdef
  a nivel de módulo.
- Cada carpeta de implementación trae su propio `Tests/` (EditMode, y PlayMode donde
  haga falta ejecutar dentro de una escena); el README explica qué mirar y en qué orden
  para cada módulo.
- Paquetes de terceros vía `Packages/manifest.json`:
  - **VContainer** (git URL de `hadashiA/VContainer`).
  - **MessagePipe**, incluyendo su paquete de integración con VContainer (git URL de
    `Cysharp/MessagePipe`).

## Convenciones compartidas

- Cada implementación alternativa de un mismo patrón vive en su propia subcarpeta con
  prefijo de **letra** (`a_...`, `b_...`, `c_...`) dentro del módulo, en orden de
  lectura sugerido de la más simple/ingenua a la más sofisticada — el prefijo
  **numérico** (`01_...`, `02_...`) queda reservado para las carpetas de módulo, que
  ordenan entre patrones distintos.
- Cada carpeta de implementación es **autocontenida**: tiene su propio Assembly
  Definition (runtime + test, y PlayMode donde aplica), y su propia copia de
  cualquier clase de dominio que en otra implementación hermana sea idéntica — no hay
  ninguna carpeta `Shared/`/`Core/` ni asmdef a nivel de módulo. Se puede copiar
  cualquiera de estas carpetas a otro proyecto Unity y compila sola, salvo por los
  paquetes de terceros (VContainer, MessagePipe, MessagePipe.VContainer, UniTask,
  uGUI), que sí es correcto que se compartan.
- Las escenas de demo son mínimas: UI de uGUI + TextMeshPro armada en el Inspector
  (Canvas, `Layout Group`, botones/texto reales, sin posiciones absolutas por pixel ni
  UI generada por código), sin arte. Cada implementación vive en su propia escena —
  tanto por necesitar estado separado (ej: `static` de un Singleton) como para poder
  compartimentalizar cada demo de forma independiente.
- La lógica que no necesita depender de `UnityEngine` se escribe en clases C# puras
  (POCOs) aunque vivan dentro del proyecto de Unity, tanto para que sean más fáciles de
  testear como para reforzar la lección de separar lógica de infraestructura ya vista en
  `clase05`.
- Tests: los asmdefs de test descriptos arriba (uno de EditMode por carpeta de
  implementación, y uno de PlayMode adicional en las que lo necesitan) se corren vía
  Unity Test Framework. El agente los corre en batchmode
  (`-runTests -testPlatform EditMode|PlayMode`) como verificación; el usuario puede
  además correrlos desde el Test Runner del Editor.

## Módulo 1 — FSM (`Assets/01_FSM/`)

### Escala chica: arma con estados

- `a_SmallBaseline/WeaponBaseline.cs`: controlador de arma con **enum + switch**
  (`Idle`, `Firing`, `Reloading`). Implementación deliberadamente corta y "sucia" —
  sirve de disparador para mostrar qué pasa cuando el número de estados/transiciones
  crece (código spaghetti, fácil de dejar un caso sin manejar).
- `b_SmallStatePattern/`: el mismo arma reconstruida sobre el motor genérico de FSM
  (ver abajo): `WeaponIdleState`, `WeaponFiringState`, `WeaponReloadingState`,
  implementando `IState`.

### Motor genérico de estados

- `IState.cs` (`OnEnter()`, `OnUpdate(float deltaTime)`, `OnExit()`) y
  `StateMachine.cs` (clase C# pura, sin `UnityEngine`, parametrizada en `TState :
  IState`, con `ChangeState(TState next)`, `Tick(float deltaTime)`, evento
  `StateChanged`): cada una de las dos implementaciones State Pattern (chica y
  grande, `b_SmallStatePattern/` y `c_LargeStatePattern/`) tiene su propia copia de
  `IState`/`StateMachine<TState>` — no hay un motor compartido a nivel de módulo. En
  `c_LargeStatePattern/`, el mismo `StateMachine<TState>` se reusa además de forma
  anidada para las substates del game flow.

### Escala grande: game flow con substates

- `c_LargeStatePattern/`: estados de alto nivel `LoadingState`, `MainMenuState`,
  `PlayingState`, cada uno `IState`, manejados por un `StateMachine<IGameFlowState>` en
  un `GameFlowController` (`MonoBehaviour`). `PlayingState` posee su **propio**
  `StateMachine` hijo para las substates pedidas en el enunciado:
  `UserPlayingState` → `PauseMenuState` → `SettingsMenuState`.
- `d_LargeScriptableObjectStates/`: la misma máquina de game flow, pero cada estado es un
  asset `ScriptableObject` (`GameFlowStateSO`, con `Enter/Exit/Tick` y referencias a los
  próximos estados posibles configurables desde el Inspector), incluyendo las mismas
  substates de `Playing`. Contraste: estados definidos por código vs. estados definidos
  por asset/diseño.

### Demo y tests

- Cuatro escenas independientes: `a_FSM_WeaponBaseline.unity` y
  `b_FSM_WeaponStatePattern.unity` (una por versión del arma, cada una con su botón
  "Fire" y su texto de debug), y `c_FSM_GameFlow.unity` / `d_FSM_GameFlow.unity` (los
  mismos botones y el mismo texto de debug del game flow en ambas — variante OOP y
  variante `ScriptableObject` respectivamente —, para que se puedan comparar lado a
  lado). En `d_FSM_GameFlow.unity`, el `GameObject` de la vista referencia los seis
  assets de estado (`LoadingState.asset`, `MainMenuState.asset`, `PlayingState.asset`,
  `UserPlayingState.asset`, `PauseMenuState.asset`, `SettingsMenuState.asset`); las
  transiciones de `PlayingState.asset` hacia sus substates están arrastradas en el
  Inspector, no hardcodeadas en código.
- Tests EditMode: transiciones del `StateMachine<TState>` genérico (se llaman
  `OnEnter`/`OnExit` en el orden correcto, no se permite una transición al mismo estado
  actual sin querer, etc.), dos suites independientes que corren la misma secuencia de
  inputs contra los mismos valores esperados hardcodeados
  (`WeaponBaselineExpectedSequenceTests`, `WeaponStatePatternExpectedSequenceTests`) —
  la comparación en runtime entre ambas implementaciones se eliminó a cambio de que cada
  carpeta sea autocontenida —, y transiciones del game flow (incluidas las substates)
  para ambas variantes (código vs. SO).

## Módulo 2 — Dependency Injection (`Assets/02_DependencyInjection/`)

### Servicios de dominio

- `IScoreService.cs` (`AddScore(int)`, `CurrentScore`, `event Action<int>
  OnScoreChanged`) e `IAudioService.cs` (`PlayCoinSound()`, implementación real
  hace `Debug.Log` en vez de sonido real, para no depender de assets de audio).
- `ScoreService.cs` / `AudioService.cs`: implementaciones plain C# de esas
  interfaces, **equivalentes en las tres variantes** (cada implementación tiene su
  propia copia de `IScoreService`/`IAudioService`/`ScoreService`/`AudioService`) — lo
  único que cambia entre implementaciones es cómo el consumidor las obtiene, para
  aislar el patrón de DI como única variable.

### Tres formas de resolver las dependencias

- `a_Singleton/`: `ScoreServiceSingleton`/`AudioServiceSingleton` como
  `MonoBehaviour` con `Instance` estático; `CoinPickupSingleton` accede vía
  `ScoreServiceSingleton.Instance`.
- `b_ServiceLocator/`: `ServiceLocator` estático genérico (`Register<T>`,
  `Resolve<T>`), un `CompositionRoot` que registra las implementaciones al arrancar la
  escena, y `CoinPickupServiceLocator` resolviendo vía `ServiceLocator.Resolve<T>()`.
- `c_VContainer/`: un `LifetimeScope` que registra `IScoreService`/`IAudioService`, y
  `CoinPickupVContainer` recibiendo ambas dependencias por **constructor injection**
  (`[Inject]`).
- Tres escenas mínimas casi idénticas (`a_DI_Singleton.unity`,
  `b_DI_ServiceLocator.unity`, `c_DI_VContainer.unity`), cada una con un botón "Coin"
  y un texto de score, para poder compararlas una al lado de la otra sin que el estado
  estático de una variante contamine a otra.

### Tests

- Tests EditMode sobre `ScoreService`/`AudioService` puros (independientes de cómo se
  resuelven).
- Un test de "wiring" por variante: que `ServiceLocator.Resolve<IScoreService>()`
  devuelva la instancia registrada, y que el `LifetimeScope` de VContainer resuelva
  correctamente `IScoreService`/`IAudioService` (via `Container.Resolve<T>()` en el
  test).

## Módulo 3 — Message Broker (`Assets/03_MessageBroker/`)

### Eventos de dominio

- `ScorePickedUpEvent.cs` (`int Amount`), `PlayerDamagedEvent.cs`
  (`int Amount`) — records/structs simples usados como payload. Cada implementación
  tiene su propia copia de los eventos que usa (`b_ScriptableObjectChannels` y
  `c_MessagePipe` solo usan `ScorePickedUpEvent`; `a_DIBroker` usa ambos).

### Tres implementaciones

- `a_DIBroker/`: broker genérico hecho a mano (`IMessageBroker` con
  `Subscribe<T>(Action<T>)` / `Publish<T>(T)` / `IDisposable` de suscripción),
  registrado como singleton en un `LifetimeScope` de VContainer e inyectado por
  constructor en un `Publisher` y un `Subscriber`. Enseña el mecanismo interno de un
  broker.
- `b_ScriptableObjectChannels/`: patrón de **event channels como asset**, muy usado en
  Unity — `ScoreEventChannelSO` / `PlayerDamagedEventChannelSO` (`ScriptableObject` con
  `event Action<int> OnRaised` y `Raise(int amount)`); `Publisher`/`Listener`
  (`MonoBehaviour`) referencian el asset desde el Inspector, sin código de por medio para
  conectar un nuevo listener.
- `c_MessagePipe/`: **MessagePipe** (Cysharp) registrado sobre el mismo `LifetimeScope`
  de VContainer (`builder.RegisterMessagePipe()`), con `Publisher`/`Subscriber` usando
  `IPublisher<T>`/`ISubscriber<T>` inyectados. Este es el ejemplo de "qué se usa en un
  juego real": mismo problema que `a_DIBroker`, resuelto con una librería madura del
  mismo ecosistema que VContainer (mejor rendimiento, filtros, soporte async/keyed
  pub-sub) en vez de reinventar el broker.
- Tres escenas mínimas, cada una con un botón que publica un evento y un texto que se
  actualiza al recibirlo.

### Tests

- Tests EditMode sobre el broker hecho a mano (múltiples suscriptores, `Dispose`
  cancela la suscripción correctamente).
- Tests EditMode sobre el `ScoreEventChannelSO` (`Raise` dispara a todos los listeners
  suscriptos).
- Un test de "wiring" para MessagePipe: resolver `IPublisher<ScorePickedUpEvent>` /
  `ISubscriber<ScorePickedUpEvent>` desde el `LifetimeScope` de test y confirmar que el
  mensaje se entrega.

## Módulo 4 — MVC → MVP → MVVM (`Assets/04_MVC_MVP_MVVM/`)

Los tres implementan la misma feature (inventario: agregar/quitar ítems por nombre) para
poder compararse directamente. Se presentan en orden **MVC → MVP → MVVM** (de más
acoplado a más desacoplado), aunque el nombre de la carpeta del módulo mantenga el orden
original del enunciado.

### Modelo y fila de lista

- `InventoryItem.cs` (`Name`, `Quantity`).
- `InventoryModel.cs`: plain C#, `AddItem(name)`, `RemoveItem(name)`,
  `IReadOnlyList<InventoryItem> Items`, evento de cambio. Equivalente en las tres
  variantes.
- `InventoryItemRow.prefab` + `InventoryItemRowView.cs`: fila de lista de la que cada
  implementación tiene su propia copia — `SetLabel(string)` actualiza el texto,
  `SetRemoveAction(UnityAction)` conecta el botón "Remove" de esa fila específica.

### Tres implementaciones

- `a_MVC/`: `InventoryController` (`MonoBehaviour`) escucha directamente los eventos de
  UI (botón Add con un `TMP_InputField`, botón Remove por fila), llama al `InventoryModel`, y
  manipula directamente los elementos concretos de la vista (instancia/destruye filas en
  un `Transform` de contenido). A propósito es la variante más acoplada y menos
  testeable — el README explica por qué eso es parte de la lección, no un descuido.
- `b_MVP/`: `IInventoryView` (interfaz pasiva: `ShowItems(...)`, eventos
  `AddRequested`/`RemoveRequested`), `InventoryPresenter` (plain C#) mediando entre el
  `InventoryModel` y `IInventoryView`, e `InventoryView` (`MonoBehaviour`) implementando
  la interfaz sin lógica propia. El presenter se testea con un `IInventoryView` fake, sin
  Unity.
- `c_MVVM/`: `InventoryViewModel` (plain C#) exponiendo una colección observable de
  `InventoryItemViewModel` y comandos (`AddCommand`/`RemoveCommand`, un `RelayCommand`
  simple tipo `Action`/`Func<bool>`); `InventoryView` (`MonoBehaviour`) solo se suscribe a
  los cambios de la colección para sincronizar filas y bindea los botones a los comandos
  — nunca llama directamente al modelo ni conoce un presenter.
- Tres escenas mínimas, cada una con un `TMP_InputField` + botón "Add" + lista con botón
  "Remove" por fila.

### Tests

- Tests EditMode sobre `InventoryModel` (una copia por variante, igual que el modelo
  que testean).
- Tests EditMode sobre `InventoryPresenter` usando un `IInventoryView` fake (agregar,
  quitar, casos de nombre vacío/duplicado).
- Tests EditMode sobre `InventoryViewModel` (los comandos mutan el modelo y disparan las
  notificaciones de cambio esperadas).
- MVC no tiene un test equivalente de "lógica de presentación" aislada — el README
  señala explícitamente que esa dificultad para testear es la consecuencia directa de
  mezclar Controller y View, cerrando la comparación entre los tres patrones.

## Verificación

- El agente que implemente este spec no interactúa con el Editor de forma gráfica.
  Verificación por fase/módulo:
  1. Crear/actualizar el proyecto y resolver paquetes corriendo Unity en
     `-batchmode -quit` (esto requiere red para bajar los paquetes de VContainer y
     MessagePipe desde sus URLs de git la primera vez).
  2. Confirmar que el proyecto compila sin errores (log de batchmode sin
     `error CS...`).
  3. Correr los tests del módulo con `-batchmode -runTests -testPlatform EditMode`
     (y `PlayMode` donde el módulo lo requiera) y confirmar que pasan.
- Queda a cargo del usuario abrir el proyecto en el Editor y validar visualmente cada
  escena de demo en Play mode (que los botones respondan, que el texto se actualice,
  que las tres variantes de cada módulo se vean equivalentes entre sí).

## Fuera de alcance

- Arte, sonido real, animaciones, UI pulida: todo queda en placeholders de uGUI y
  `Debug.Log`.
- Persistencia (guardado/carga) en cualquiera de los módulos.
- Integración entre módulos (por ejemplo, que el Message Broker dispare transiciones de
  la FSM grande): cada módulo es autocontenido a propósito.
- Multiplayer / networking.
- CI automatizado para correr los tests de Unity en cada push (podría ser un follow-up,
  no parte de esta clase).
