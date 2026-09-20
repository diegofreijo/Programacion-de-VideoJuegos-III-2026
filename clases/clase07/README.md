# Clase 07 — Patrones de diseño para juegos: FSM, DI, Message Broker, MVC/MVP/MVVM

## Objetivo

Este proyecto de Unity muestra, para cuatro familias de patrones muy usadas en
gameplay, al menos dos implementaciones concretas y comparables del mismo problema:
desde la versión más ingenua/rápida de escribir hasta la más productiva/usada en la
industria. La idea es poder abrir dos o tres escenas casi idénticas, ver que se
comportan igual desde afuera, y discutir el trade-off de cómo están armadas por
adentro. Diseño completo, decisiones de alcance y detalle de cada módulo en
[`SPEC.md`](SPEC.md).

## Estructura

Todo vive en un único proyecto real de Unity, `Unity/` (no hay stubs ni simulación
como en `clase05`: VContainer y MessagePipe son paquetes de Unity y tiene sentido
verlos correr en el Editor).

- `Unity/Packages/manifest.json`: dependencias de terceros vía git URL — VContainer,
  MessagePipe, MessagePipe.VContainer y UniTask.
- `Unity/Assets/01_FSM/`: máquina de estados finitos, a escala chica (un arma) y
  grande (game flow con submáquinas anidadas), cada una en `Baseline`/`StatePattern`
  o `StatePattern`/`ScriptableObjectStates`, más un `Core/` con el motor genérico
  reusable.
- `Unity/Assets/02_DependencyInjection/`: mismo par de servicios (`IScoreService`,
  `IAudioService`) resuelto de tres formas — `01_Singleton/`, `02_ServiceLocator/`,
  `03_VContainer/` — con el dominio compartido en `Shared/`.
- `Unity/Assets/03_MessageBroker/`: mismos eventos de dominio publicados/escuchados
  de tres formas — `01_DIBroker/` (broker hecho a mano), `02_ScriptableObjectChannels/`
  (event channels como asset), `03_MessagePipe/` (librería madura del mismo
  ecosistema que VContainer) — con `Shared/` para los eventos.
- `Unity/Assets/04_MVC_MVP_MVVM/`: la misma feature de inventario (agregar/quitar
  ítems) implementada en `01_MVC/`, `02_MVP/`, `03_MVVM/`, en orden de menor a mayor
  desacoplamiento, con el modelo compartido en `Shared/`.
- Cada módulo tiene su propio Assembly Definition (`Clase07.FSM`, `Clase07.DI`,
  `Clase07.MessageBroker`, `Clase07.Mvx`) y su propia carpeta `Tests/`
  (EditMode y, donde hace falta, PlayMode).

## Cómo abrir el proyecto

1. Abrir Unity Hub y agregar la carpeta `clases/clase07/Unity` como proyecto (no
  `clases/clase07`: el `.sln`/`Assets` real está un nivel más adentro).
2. Usar **Unity 6000.3.21f1** — es la versión con la que se creó el proyecto
  (`Unity/ProjectSettings/ProjectVersion.txt`). Si Hub no la tiene instalada, la
  ofrece instalar sola al abrir el proyecto.
3. La primera vez que se abre, el Package Manager resuelve VContainer, MessagePipe,
  MessagePipe.VContainer y UniTask desde sus URLs de git en `Packages/manifest.json`
  — **hace falta acceso a red** para esa primera resolución (después queda cacheado
  localmente). Si el Editor muestra errores de compilación apenas abre, esperar a que
  termine de resolver paquetes antes de asumir que algo está roto.

## Cómo correr cada módulo

Todas las escenas son mínimas (botones y texto de debug, sin arte): abrir la escena
indicada y entrar en Play mode.

- **01 — FSM:** `Assets/01_FSM/01_FSM_Demo.unity` — botón "Fire" para las dos
  versiones del arma (baseline vs. motor de estados) montadas una al lado de la
  otra, y botones para avanzar el game flow (`Load`, `Play`, `Pause`,
  `Open Settings`, `Resume`, etc.) en sus dos variantes (estados por código vs. por
  `ScriptableObject`), con texto de debug mostrando el estado activo de cada máquina.
- **02 — Dependency Injection**, tres escenas casi idénticas (botón "Coin" + texto
  de score) que conviene correr una por vez, ya que alguna variante usa estado
  `static`:
  - `Assets/02_DependencyInjection/01_Singleton/01_DI_Singleton.unity`
  - `Assets/02_DependencyInjection/02_ServiceLocator/02_DI_ServiceLocator.unity`
  - `Assets/02_DependencyInjection/03_VContainer/03_DI_VContainer.unity`
- **03 — Message Broker**, tres escenas casi idénticas (botón que publica un evento
  + texto que se actualiza al recibirlo):
  - `Assets/03_MessageBroker/01_DIBroker/01_MessageBroker_DIBroker.unity`
  - `Assets/03_MessageBroker/02_ScriptableObjectChannels/02_MessageBroker_SOChannels.unity`
  - `Assets/03_MessageBroker/03_MessagePipe/03_MessageBroker_MessagePipe.unity`
- **04 — MVC / MVP / MVVM**, tres escenas casi idénticas (`InputField` + botón "Add"
  + lista con botón "Remove" por fila):
  - `Assets/04_MVC_MVP_MVVM/01_MVC/01_Mvx_MVC.unity`
  - `Assets/04_MVC_MVP_MVVM/02_MVP/02_Mvx_MVP.unity`
  - `Assets/04_MVC_MVP_MVVM/03_MVVM/03_Mvx_MVVM.unity`

## Cómo correr los tests

**Desde el Editor:** `Window > General > Test Runner`, pestañas `EditMode` y
`PlayMode`, "Run All". La mayoría de los tests de este proyecto son EditMode; hay
PlayMode donde el módulo necesita ejecutar dentro de una escena (por ejemplo el
motor de FSM en `01_FSM/Tests/PlayMode`).

**Desde línea de comandos (batchmode)**, sin abrir el Editor de forma interactiva —
parado en la raíz del monorepo del curso. Importante: **no combinar `-runTests` con
`-quit`** (compiten por cerrar el Editor y el run puede no llegar a ejecutarse), y
pasar rutas absolutas (`$(pwd)/...`) a `-testResults`/`-logFile`, porque Unity las
resuelve contra su propio `-projectPath` interno y no contra el directorio desde el
que se lo invoca:

```bash
# EditMode
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform EditMode \
  -testResults "$(pwd)/clases/clase07/Unity_editmode_results.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_editmode.log"

# PlayMode
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographic \
  -projectPath clases/clase07/Unity \
  -runTests -testPlatform PlayMode \
  -testResults "$(pwd)/clases/clase07/Unity_playmode_results.xml" \
  -logFile "$(pwd)/clases/clase07/Unity_playmode.log"
```

Ambos comandos terminan solos al finalizar el test run (código de salida `0` si
todo pasó); revisar el XML resultante o buscar `result="Failed"` en él.

## Qué mirar en cada módulo

Resumen del contraste entre implementaciones; detalle completo (por qué cada
decisión, qué testea cada suite) en [`SPEC.md`](SPEC.md).

- **01 — FSM:** el arma baseline (`enum` + `switch`) contra la misma arma sobre el
  motor genérico de estados (`IState` + `StateMachine<TState>`) — mismo
  comportamiento, pero el baseline es el disparador para hablar de código spaghetti
  a medida que crecen estados/transiciones. A escala grande, el game flow con
  submáquinas anidadas (menú de pausa dentro de "Playing") se resuelve dos veces:
  estados definidos por código vs. estados definidos como asset de
  `ScriptableObject` configurable desde el Inspector.
- **02 — Dependency Injection:** el mismo `IScoreService`/`IAudioService` obtenido de
  tres formas — `Instance` estático de un Singleton, un `ServiceLocator` genérico
  con `Resolve<T>()`, y constructor injection real vía VContainer (`[Inject]`) desde
  un `LifetimeScope`. Lo único que cambia entre las tres variantes es cómo el
  consumidor consigue la dependencia; el servicio en sí es idéntico.
- **03 — Message Broker:** el mismo par de eventos publicado/escuchado con un
  broker hecho a mano (`IMessageBroker.Subscribe<T>/Publish<T>`, para entender el
  mecanismo interno), con event channels como `ScriptableObject` (patrón muy usado
  en Unity, cero código para conectar un nuevo listener), y con MessagePipe (la
  solución madura del mismo ecosistema que VContainer, con mejor rendimiento y
  soporte async/keyed pub-sub en vez de reinventar el broker).
- **04 — MVC → MVP → MVVM:** la misma feature de inventario, de más acoplada a más
  testeable. MVC mezcla a propósito el control de eventos de UI con la
  manipulación directa de la vista (y por eso no tiene un test de lógica de
  presentación aislada — la dificultad de testearlo es la lección). MVP introduce
  una interfaz pasiva de vista (`IInventoryView`) y un presenter testeable con un
  fake, sin Unity. MVVM va un paso más allá: un `ViewModel` plain C# expone estado
  observable y comandos (`RelayCommand`), y la vista solo se suscribe/bindea, sin
  conocer modelo ni presenter.
