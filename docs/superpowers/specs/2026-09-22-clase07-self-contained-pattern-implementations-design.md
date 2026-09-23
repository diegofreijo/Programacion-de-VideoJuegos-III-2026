# Clase07: hacer que cada implementación de patrón sea autocontenida y portable

## Contexto y problema

`clases/clase07/Unity` muestra cuatro familias de patrones (FSM, Dependency Injection,
Message Broker, MVC/MVP/MVVM), cada una con 2-4 implementaciones alternativas del mismo
problema. Hoy cada implementación ya vive en su propia escena, pero **no** es
independiente del resto del módulo:

- Cada módulo compila con un único par de asmdefs (`Clase07.<Modulo>` +
  `Clase07.<Modulo>.Tests`) que mezcla el código de **todas** sus implementaciones en el
  mismo ensamblado.
- El dominio compartido entre implementaciones vive en una carpeta común por módulo —
  `Shared/` en DI, MessageBroker y MVx; `Core/` en FSM — referenciada por todas las
  variantes.
- Algunos tests comparan directamente dos implementaciones en el mismo archivo (ej.
  `WeaponEquivalenceTests` referencia tanto `WeaponBaseline` como
  `WeaponStatePatternController`).

Consecuencia: no se puede tomar la carpeta de una sola implementación (ej.
`02_DependencyInjection/02_ServiceLocator/`) y moverla tal cual a otro proyecto — le
faltarían `Shared/`, el asmdef del módulo, y potencialmente compilaría junto con código
de otras implementaciones que no hacen falta. Esto también dificulta la lectura: para
entender una sola implementación hay que saltar entre su carpeta y la carpeta compartida
del módulo.

## Objetivo

- Cada carpeta de implementación (ej. `a_Singleton/`, `b_ScriptableObjectChannels/`,
  `c_MVVM/`) queda **autocontenida**: contiene su escena, todas las clases que necesita
  para funcionar (incluyendo copias propias de lo que hoy es código compartido del
  módulo), su propio `.asmdef` runtime y su propia carpeta `Tests/` con `.asmdef` de
  test. Se puede copiar esa carpeta entera a otro proyecto Unity y compila y corre sola,
  con la única excepción de paquetes de terceros vía git URL (VContainer, MessagePipe,
  MessagePipe.VContainer, UniTask, uGUI), que siguen siendo dependencias de
  `Packages/manifest.json` y es correcto que se compartan.
- Las carpetas de implementación llevan además un prefijo de **letra** (`a_`, `b_`,
  `c_`, ...) que indica el orden de lectura sugerido dentro del módulo — de la variante
  más simple/ingenua a la más sofisticada — para no confundirse con el prefijo numérico
  de las carpetas de módulo (`01_FSM`, `02_DependencyInjection`, ...), que ordena entre
  patrones distintos.
- En los puntos de cada implementación que son el motivo real de mostrarla (la línea o
  bloque que hace a esa variante distinta de sus hermanas), se agrega o refuerza un
  comentario corto explicando qué mirar ahí.

## Fuera de alcance

- No se cambia el comportamiento observable de ninguna implementación, ni se agregan
  escenas nuevas (`d_LargeScriptableObjectStates` de FSM sigue sin escena — sigue siendo
  una decisión de alcance deliberada, ya documentada).
- No se cambia la UI de las escenas (ya armada en el Inspector, ver
  `2026-09-21-clase07-demo-ui-inspector-migration-design.md`) más allá de lo que implica
  mover/renombrar el archivo `.unity`.
- No se agregan tests nuevos de comportamiento — los tests existentes se mueven,
  duplican o dividen según la sección "Tests cruzados", pero la cobertura conceptual es
  la misma.
- No se integra ningún módulo con otro (sigue siendo alcance explícito del `SPEC.md`
  original que los cuatro módulos son independientes entre sí).

## Convención de nombres

- **Número de carpeta de módulo** (`01_FSM`, `02_DependencyInjection`,
  `03_MessageBroker`, `04_MVC_MVP_MVVM`): orden entre patrones distintos. No cambia.
- **Letra de carpeta de implementación** (`a_`, `b_`, `c_`, ...), minúscula, separador
  `_`: orden de lectura sugerido entre variantes alternativas del mismo patrón, de más
  simple/ingenua a más sofisticada/productiva. Nueva.
- El archivo `.unity` de cada implementación se renombra con el mismo prefijo de letra
  que su carpeta (vía `git mv`, preservando el `.meta`/GUID).
- Dentro de FSM, la agrupación intermedia por escala (`Small/`, `Large/`) **se elimina**:
  las cuatro implementaciones quedan directamente bajo `01_FSM/`, con una única
  secuencia de letras `a_`/`b_`/`c_`/`d_` para todo el módulo (igual que en los otros
  tres módulos), en vez de una secuencia de letras por grupo de escala anidada dos
  niveles bajo `01_FSM/`. El nombre de cada carpeta conserva la escala en el propio
  nombre (`a_SmallBaseline`, `b_SmallStatePattern`, `c_LargeStatePattern`,
  `d_LargeScriptableObjectStates`) para no perder la comparación "arma chica entre sí,
  game flow grande entre sí" que daba sentido a la agrupación original — solo cambia
  dónde vive esa información (nombre de carpeta en vez de carpeta intermedia).

## Estructura final por módulo

Formato: `carpeta/` → `namespace` (= nombre del asmdef runtime; el de test agrega
`.Tests`, el de PlayMode agrega `.Tests.PlayMode`) → qué se le copia adentro que hoy
está en `Shared/`/`Core/`.

### 01_FSM

| Carpeta | Namespace / asmdef | Se copia adentro |
|---|---|---|
| `a_SmallBaseline/` (antes `Small/Baseline/`) | `Clase07.FSM.SmallBaseline` | nada (ya autocontenida) |
| `b_SmallStatePattern/` (antes `Small/StatePattern/`) | `Clase07.FSM.SmallStatePattern` | `IState.cs`, `StateMachine.cs` (copia propia, mismo namespace que el resto del archivo) |
| `c_LargeStatePattern/` (antes `Large/StatePattern/`) | `Clase07.FSM.LargeStatePattern` | `IState.cs`, `StateMachine.cs` (copia propia, independiente de la de `b_SmallStatePattern/`) |
| `d_LargeScriptableObjectStates/` (antes `Large/ScriptableObjectStates/`) | `Clase07.FSM.LargeScriptableObjectStates` | nada (ya autocontenida) |

Las cuatro carpetas quedan directamente bajo `01_FSM/` — sin la agrupación intermedia
`Small/`/`Large/` que tenían antes; esa información pasa a formar parte del nombre de
cada carpeta. `Core/` se borra una vez que las dos copias existen y compilan. Escenas:
`a_SmallBaseline/a_FSM_WeaponBaseline.unity`,
`b_SmallStatePattern/b_FSM_WeaponStatePattern.unity`,
`c_LargeStatePattern/c_FSM_GameFlow.unity` (`d_LargeScriptableObjectStates/` sigue sin
escena).

### 02_DependencyInjection

| Carpeta | Namespace / asmdef | Se copia adentro |
|---|---|---|
| `a_Singleton/` (antes `01_Singleton/`) | `Clase07.DI.Singleton` | `IScoreService.cs`, `IAudioService.cs`, `ScoreService.cs`, `AudioService.cs` |
| `b_ServiceLocator/` (antes `02_ServiceLocator/`) | `Clase07.DI.ServiceLocatorPattern` | ídem |
| `c_VContainer/` (antes `03_VContainer/`) | `Clase07.DI.VContainerExample` | ídem |

Las cuatro clases copiadas cambian su namespace de `Clase07.DI.Shared` al namespace
propio de cada implementación (el mismo que ya usan sus archivos actuales) — dejan de
llamarse "Shared" porque ya no lo son, son la copia local de esa carpeta. `Shared/` se
borra una vez que las tres copias existen y compilan. Escenas: `a_DI_Singleton.unity`,
`b_DI_ServiceLocator.unity`, `c_DI_VContainer.unity`.

### 03_MessageBroker

| Carpeta | Namespace / asmdef | Se copia adentro |
|---|---|---|
| `a_DIBroker/` (antes `01_DIBroker/`) | `Clase07.MessageBroker.DIBroker` | `ScorePickedUpEvent.cs`, `PlayerDamagedEvent.cs` |
| `b_ScriptableObjectChannels/` (antes `02_ScriptableObjectChannels/`) | `Clase07.MessageBroker.ScriptableObjectChannels` | `ScorePickedUpEvent.cs` (hoy es la única que usa esta carpeta — no se agrega `PlayerDamagedEvent.cs` ahí, es una asimetría de alcance preexistente que no corresponde arreglar en esta migración) |
| `c_MessagePipe/` (antes `03_MessagePipe/`) | `Clase07.MessageBroker.MessagePipeExample` | `ScorePickedUpEvent.cs`, `PlayerDamagedEvent.cs` |

Mismo criterio de namespace que en DI (las copias toman el namespace de su
implementación). `Shared/` se borra al final. Escenas:
`a_MessageBroker_DIBroker.unity`, `b_MessageBroker_SOChannels.unity`,
`c_MessageBroker_MessagePipe.unity`.

### 04_MVC_MVP_MVVM

| Carpeta | Namespace / asmdef | Se copia adentro |
|---|---|---|
| `a_MVC/` (antes `01_MVC/`) | `Clase07.Mvx.Mvc` | `InventoryItem.cs`, `InventoryModel.cs`, `InventoryItemRowView.cs` + prefab `InventoryItemRow.prefab` |
| `b_MVP/` (antes `02_MVP/`) | `Clase07.Mvx.Mvp` | ídem |
| `c_MVVM/` (antes `03_MVVM/`) | `Clase07.Mvx.Mvvm` | ídem |

El prefab `InventoryItemRow.prefab` es un asset de Unity, no solo un script: se
duplica como archivo (`.prefab` + `.meta` nuevo) en cada carpeta, no se referencia
desde una ubicación compartida. `Shared/` se borra al final. Escenas: `a_Mvx_MVC.unity`,
`b_Mvx_MVP.unity`, `c_Mvx_MVVM.unity`.

## Tests cruzados

Dos casos en los que un test hoy referencia código de más de una implementación en el
mismo archivo — dejan de poder vivir en una sola carpeta autocontenida:

- **`WeaponEquivalenceTests`** (`01_FSM/Tests/`): hoy corre la misma secuencia de
  inputs sobre `WeaponBaseline` y `WeaponStatePatternController` y compara sus
  resultados en runtime. Se reemplaza por dos suites independientes — una dentro de
  `a_SmallBaseline/Tests/`, otra dentro de `b_SmallStatePattern/Tests/` — que corren
  la misma secuencia de inputs contra **los mismos valores esperados hardcodeados**
  (extraídos del comportamiento actual, ya verificado). Se pierde la comparación
  cruzada en runtime a cambio de que cada carpeta quede autosuficiente.
- **`FsmDemoViewsPlayModeTests`** (`01_FSM/Tests/PlayMode/`): hoy carga las tres
  escenas de FSM en un solo archivo y clickea todos los botones de cada una. Se parte
  en un test PlayMode por implementación (uno en `a_SmallBaseline/Tests/PlayMode/`,
  otro en `b_SmallStatePattern/Tests/PlayMode/`, otro en
  `c_LargeStatePattern/Tests/PlayMode/`), cada uno cargando y verificando solo su
  propia escena.

El resto de los tests actuales (`StateMachineTests`, `GameFlowControllerTests`,
`GameFlowSORunnerTests`, los tests de dominio de DI/MessageBroker/MVx, los tests de
wiring por variante) ya son autocontenidos hoy — no referencian tipos de otra
implementación — así que solo se mudan de carpeta (y, donde el test ejercita una clase
de dominio que se está copiando, ej. `ScoreServiceTests` sobre `ScoreService`, el test
se copia igual que la clase que testea: una copia por implementación, mismo criterio que
el código de producción).

## Comentarios pedagógicos

Se agrega (o se refuerza, donde ya existe algo parecido) un comentario corto en el
punto exacto de cada implementación que es la razón de mostrarla — no una descripción
genérica de la clase, sino qué mirar ahí y por qué es distinto de las otras variantes
del mismo patrón:

- **FSM chico** — `a_SmallBaseline`: el `switch` de `WeaponBaseline.Tick`/`PressTrigger`
  (ya crece con cada estado nuevo). `b_SmallStatePattern`: el `ChangeState(...)` dentro
  de cada estado (`WeaponIdleState`, etc.) — cada transición vive en el estado que la
  dispara, no en un switch central.
- **FSM grande** — `c_LargeStatePattern`: el `_substateMachine` dentro de `PlayingState`
  — una máquina de estados adentro de un estado. `d_LargeScriptableObjectStates`: los
  `[SerializeField]` de `PlayingStateSO` apuntando a los siguientes estados — la
  transición queda configurable desde el Inspector, no hardcodeada en código.
- **DI** — `a_Singleton`: el `Instance` estático. `b_ServiceLocator`: la llamada a
  `ServiceLocator.Resolve<T>()` en el consumidor. `c_VContainer`: el constructor con
  `[Inject]` — la dependencia se declara, no se busca.
- **MessageBroker** — `a_DIBroker`: `Subscribe<T>`/`Publish<T>` y el `IDisposable` que
  hay que guardar para poder cancelar la suscripción. `b_ScriptableObjectChannels`: el
  campo `[SerializeField]` que referencia el asset del canal — cablear un nuevo
  listener no toca código. `c_MessagePipe`: `IPublisher<T>`/`ISubscriber<T>` inyectados
  — mismo problema que `a_DIBroker`, resuelto por una librería en vez de código propio.
- **MVx** — `a_MVC`: donde el controller manipula directamente los elementos de la
  vista (instancia/destruye filas) — el punto que lo hace difícil de testear.
  `b_MVP`: la interfaz `IInventoryView` y cómo el presenter solo habla con ella, nunca
  con Unity. `c_MVVM`: el binding de la vista a `RelayCommand`/la colección observable
  — la vista nunca llama al modelo directamente.

## Documentación a actualizar

- `clases/clase07/README.md`: todas las rutas de escena (sección "Cómo correr cada
  módulo"), y agregar la convención de letras a la sección "Estructura".
- `clases/clase07/SPEC.md`: la sección "Convenciones compartidas" agrega la regla de
  autocontención total por implementación (asmdef propio, sin `Shared/`/`Core/` de
  módulo) y la convención de letras; cada sección de módulo actualiza sus rutas de
  carpeta/asmdef al nuevo esquema.
- `clases/clase07/PLAN.md`: ya lleva una nota histórica al principio marcándolo como
  desactualizado respecto a `SPEC.md`/`README.md` — no hace falta tocarlo, el plan de
  implementación de esta migración es un documento nuevo (ver próximo paso).

## Migración y verificación

- Un módulo por vez, mismo orden que el `PLAN.md` original: FSM, DI, MessageBroker,
  MVx. Un commit por módulo (o por implementación dentro del módulo, si el diff queda
  más legible así).
- Por implementación: `git mv` de carpeta y escena a su nombre con letra; asmdef
  runtime y de test nuevos; copiar (no mover) las clases de `Shared/`/`Core/` que usa,
  ajustando su namespace al de la implementación; copiar/dividir sus tests según la
  sección "Tests cruzados"; agregar los comentarios pedagógicos de la sección anterior.
  Recién se borra `Shared/`/`Core/` y el asmdef de módulo cuando ninguna implementación
  restante los referencia.
- Verificación por módulo, igual mecanismo que ya usa el proyecto (sin abrir el Editor
  de forma interactiva): batchmode compile, después `-runTests -testPlatform EditMode`
  (y `PlayMode` para FSM), confirmando en el log `Failed: 0` y ausencia de `error CS`
  antes de commitear.

## Riesgos / notas de migración

- Duplicar clases de dominio significa que un bug arreglado en una copia no se
  propaga automáticamente a las otras — es el costo aceptado a cambio de portabilidad
  total; vale la pena dejarlo explícito en el README para quien extienda el proyecto
  más adelante.
- El conteo de asmdefs sube de 9 a ~26-28 (uno runtime + uno de test por
  implementación, más los `.Tests.PlayMode` de FSM). Es más ruido en el Project
  window, pero es exactamente lo que hace que cada carpeta compile de forma aislada.
- Al copiar `InventoryItemRow.prefab` tres veces, cada copia necesita su propio `.meta`
  (GUID distinto) — no se debe copiar el `.meta` original, Unity lo regenera al
  importar el prefab nuevo en cada carpeta.
