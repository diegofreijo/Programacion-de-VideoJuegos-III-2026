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
  grande (game flow con submáquinas anidadas), en cuatro carpetas de implementación —
  `a_SmallBaseline/`, `b_SmallStatePattern/`, `c_LargeStatePattern/`,
  `d_LargeScriptableObjectStates/`.
- `Unity/Assets/02_DependencyInjection/`: mismo par de servicios (`IScoreService`,
  `IAudioService`) resuelto de tres formas — `a_Singleton/`, `b_ServiceLocator/`,
  `c_VContainer/`.
- `Unity/Assets/03_MessageBroker/`: mismos eventos de dominio publicados/escuchados
  de tres formas — `a_DIBroker/` (broker hecho a mano), `b_ScriptableObjectChannels/`
  (event channels como asset), `c_MessagePipe/` (librería madura del mismo
  ecosistema que VContainer).
- `Unity/Assets/04_MVC_MVP_MVVM/`: la misma feature de inventario (agregar/quitar
  ítems) implementada en `a_MVC/`, `b_MVP/`, `c_MVVM/`, en orden de menor a mayor
  desacoplamiento.

Dentro de cada módulo, cada implementación alternativa vive en su propia carpeta con
prefijo de **letra** (`a_`, `b_`, `c_`, ...), en orden de lectura sugerido de la más
simple/ingenua a la más sofisticada — a diferencia del prefijo **numérico** de las
carpetas de módulo (`01_FSM`, `02_DependencyInjection`, ...), que ordena entre
patrones distintos. Cada carpeta de implementación es autocontenida: tiene su propio
Assembly Definition, su propia copia de cualquier clase de dominio que antes era
compartida dentro del módulo, y su propia carpeta `Tests/` — se puede copiar cualquiera
de esas carpetas a otro proyecto Unity y compila sola, salvo por los paquetes de
terceros (VContainer, MessagePipe, UniTask, uGUI), que sí es correcto que se
compartan (con la salvedad de que los tests PlayMode de FSM en `a_SmallBaseline`,
`b_SmallStatePattern` y `c_LargeStatePattern` tienen hardcodeada internamente la ruta
completa `Assets/...` de su escena, así que tras copiar la carpeta esos tests puntuales
necesitarían actualizar esa ruta para volver a pasar, aunque la carpeta siga
compilando).

- Cada implementación tiene su propio Assembly Definition (ej. `Clase07.DI.Singleton`,
  `Clase07.MessageBroker.MessagePipeExample`, `Clase07.Mvx.Mvvm`) y su propia carpeta
  `Tests/` (EditMode y, donde hace falta, PlayMode) — ya no hay un asmdef ni una
  carpeta `Shared/`/`Core/` a nivel de módulo.

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

Todas las escenas están armadas en el Inspector (Canvas + TextMeshPro + `Layout Group`,
sin posiciones absolutas por pixel), con la UI mínima pero prolija: abrir la escena
indicada y entrar en Play mode.

- **01 — FSM**, tres escenas independientes:
  - `Assets/01_FSM/a_SmallBaseline/a_FSM_WeaponBaseline.unity` — botón "Fire" para la
    versión del arma con `enum` + `switch`, con texto de debug mostrando estado y
    munición.
  - `Assets/01_FSM/b_SmallStatePattern/b_FSM_WeaponStatePattern.unity` — el mismo
    arma sobre el motor genérico de estados (`IState` + `StateMachine<TState>`, copia
    local de esta carpeta), en su propia escena.
  - `Assets/01_FSM/c_LargeStatePattern/c_FSM_GameFlow.unity` — botones para avanzar
    el game flow (`Finish Loading`, `Play`, `Pause`, `Open Settings`, `Close
    Settings`, `Resume`), con texto de debug mostrando el estado y el subestado
    activos. La escena muestra **sólo** la variante OOP del game flow; la variante con
    estados como `ScriptableObject`
    (`Assets/01_FSM/d_LargeScriptableObjectStates/`) se verifica por tests de
    EditMode en vez de montarse también en una escena — sigue siendo una decisión de
    alcance deliberada, ver [`SPEC.md`](SPEC.md).
- **02 — Dependency Injection**, tres escenas casi idénticas (botón "Coin" + texto
  de score) que conviene correr una por vez, ya que alguna variante usa estado
  `static`:
  - `Assets/02_DependencyInjection/a_Singleton/a_DI_Singleton.unity`
  - `Assets/02_DependencyInjection/b_ServiceLocator/b_DI_ServiceLocator.unity`
  - `Assets/02_DependencyInjection/c_VContainer/c_DI_VContainer.unity`
- **03 — Message Broker**, tres escenas casi idénticas (botón que publica un evento
  + texto que se actualiza al recibirlo):
  - `Assets/03_MessageBroker/a_DIBroker/a_MessageBroker_DIBroker.unity`
  - `Assets/03_MessageBroker/b_ScriptableObjectChannels/b_MessageBroker_SOChannels.unity`
  - `Assets/03_MessageBroker/c_MessagePipe/c_MessageBroker_MessagePipe.unity`
- **04 — MVC / MVP / MVVM**, tres escenas casi idénticas (`TMP_InputField` + botón "Add"
  + lista con botón "Remove" por fila):
  - `Assets/04_MVC_MVP_MVVM/a_MVC/a_Mvx_MVC.unity`
  - `Assets/04_MVC_MVP_MVVM/b_MVP/b_Mvx_MVP.unity`
  - `Assets/04_MVC_MVP_MVVM/c_MVVM/c_Mvx_MVVM.unity`

## Cómo correr los tests

**Desde el Editor:** `Window > General > Test Runner`, pestañas `EditMode` y
`PlayMode`, "Run All". La mayoría de los tests de este proyecto son EditMode; hay
PlayMode donde la implementación necesita ejecutar dentro de una escena, por ejemplo
en el motor de FSM: `01_FSM/a_SmallBaseline/Tests/PlayMode/`,
`01_FSM/b_SmallStatePattern/Tests/PlayMode/` y
`01_FSM/c_LargeStatePattern/Tests/PlayMode/`.

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

  Una asimetría a tener en cuenta al leer el código: en las variantes Singleton y
  Service Locator, un único componente hace las dos cosas —
  `DiSingletonDemoBootstrapper` / `DiServiceLocatorDemoBootstrapper` arman los
  servicios *y* los consumen en el mismo `Build()`, por brevedad. La variante
  VContainer, en cambio, las separa: `DiVContainerLifetimeScope` es el composition
  root (sólo registra) y `CoinPickupVContainer` es el consumidor (sólo recibe lo
  registrado, por `[Inject]`). Esa separación es justamente cómo se estructura un
  proyecto real con VContainer — composition root aparte de los consumidores — y es
  parte de lo que se gana al pasar del acceso global a la inyección: el consumidor
  deja de saber de dónde salen sus dependencias, así que también se puede testear
  con dobles sin tocar la escena.
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
