**Tier:** Job · **Tickets:** 1 · **Pause points:** none · **Pick:** none · **Review:** none · **Landing:** Merged PR

## Problem
En `clases/clase14` el código está en dos carpetas planas (`Runtime/`, `Editor/`): conviven un
componente de juego (`Turret`) con un tipo de validación (`FloatRange`), y cuesta ubicar qué archivo
muestra cada tema. El diálogo de Graph Toolkit solo se imprime en la consola: no se ve cómo lo usa un
juego.

## Solution
Una carpeta por tema de la clase, numerada en el orden en que se explica, cada una con su `Runtime/`
y/o `Editor/` y un asmdef propio. Una escena nueva, `Dialogo.unity`, donde el jugador camina hasta
el Guardia y conversa con él usando `Guardia.dialogue`. Más ayudas para exponer: recorrido en el
README, `LEEME.md` por carpeta, menú para saltar de escena y un HUD en `Main`.

## Behaviours
1. Como alumno, quiero encontrar el código de cada tema en una carpeta numerada en el orden de la clase (`01-ScriptableObjects`, `02-Atributos`, `03-Inspectors`, `04-Ventanas`, `05-GraphToolkit`, `06-Juego`, `07-Tests`, `08-Build`), cada una con un asmdef por lado (Runtime/Editor).
2. Como alumno, quiero leer un `LEEME.md` corto en cada carpeta de tema que diga qué muestra y qué archivo abrir primero.
3. Como alumno, quiero abrir `Scenes/Dialogo.unity`, mover al jugador con WASD/flechas hasta el Guardia, apretar E y leer el diálogo en una caja en pantalla, eligiendo opciones con 1/2 y avanzando con Espacio/E.
4. Como docente, quiero saltar entre Main, Diálogo y Profiler desde el menú **Clase14 > Escenas**.
5. Como alumno, quiero ver en `Main` un HUD con la oleada actual, los enemigos vivos y la vida del objetivo.
6. Como alumno, quiero que el README recorra los temas en el orden de la clase con las rutas nuevas.

## Decisions
- Los archivos se mueven con su `.meta`: los GUID no cambian y escenas y assets siguen enlazados. Los namespaces (`Clase14`, `Clase14.Editor`, `Clase14.Editor.Dialogue`) no cambian.
- Un asmdef por tema y lado (`Clase14.<Tema>` y `Clase14.<Tema>.Editor`), sin ciclos. El Inspector de oleadas abre la ventana de balanceo por su menú (`EditorApplication.ExecuteMenuItem`) en vez de referenciar la clase, para que `Inspectors` no dependa de `Ventanas`.
- `Data/`, `Scenes/` y `Settings/` quedan en la raíz de `Clase14/`. `SampleContent` (en `08-Build`) genera también la escena `Dialogo` y la agrega al build.
- La caja de diálogo y el HUD usan `TextMesh` en el mundo (núcleo de Unity): sin paquetes nuevos y se ven en el build. Input Manager clásico, como el resto del proyecto.
- El diálogo se maneja desde métodos públicos (`StartTalking`, `Continue`, `Choose`) que lee el `Update`; el test de Play Mode los llama directo.
- `DialoguePlayer` (consola) queda en `Main` como estaba.

## Acceptance
- Tests de Edit Mode y Play Mode pasan, con un test de Play Mode nuevo que recorre una rama del diálogo.
- `Build.ps1` termina bien con las tres escenas.
- Captura batch de `Dialogo.unity` con la caja abierta mostrando la pregunta y sus dos opciones.
