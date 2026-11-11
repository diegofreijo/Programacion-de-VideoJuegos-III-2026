**Tier:** Job · **Tickets:** 1 · **Pause points:** none · **Pick:** none · **Review:** none · **Landing:** Merged PR

## Problem
El único grafo de Graph Toolkit de clase14 es el diálogo, que es casi una lista con ramas: no
muestra por qué hace falta un grafo. Para la clase falta un segundo ejemplo corto (menos de 15
minutos) cuyos datos **solo** se puedan modelar con un grafo: con un ciclo, y con caminos que
convergen. Catálogo de ideas: `docs/ideas/clase14-grafos-custom.md` (#19).

## Solution
Un grafo `.turretcycle` que describe el ciclo de sobrecalentamiento de la torreta como máquina de
estados: nodos **Inicio** y **Estado**, y transiciones como salidas del Estado (como las opciones
del `ChoiceNode`). Igual que el diálogo, el grafo no se ejecuta: un `ScriptedImporter` lo compila a
un `TurretCycleAsset` plano, una clase pura `TurretCycleRunner` lo recorre, y un componente
`TurretCycle` maneja a la `Turret` en una escena nueva, **Torreta**.

Ciclo de ejemplo (generado por `SampleContent`):

```
        ┌── Disparos ≥ 10 ──▶ [Recalentada]
[Activa]                         │ Pasaron 2 s
   ▲                             ▼
   └─ Pasaron 3 s / Sin enemigos ─ [Enfriando]
```

- **Activa**: dispara, cadencia ×1, amarilla.
- **Recalentada**: no dispara, roja.
- **Enfriando**: dispara, cadencia ×0.5, naranja.

## Behaviours
1. Como docente, quiero crear un ciclo desde **Assets > Create > Clase14 > Ciclo de torreta** y
   editarlo en la ventana de Graph Toolkit con los nodos **Inicio** y **Estado**.
2. Como diseñador, quiero configurar en cada Estado si **puede disparar**, su **multiplicador de
   cadencia** y su **color**, y con la opción **Transiciones** (cantidad) agregarle salidas.
3. Como diseñador, quiero elegir para cada transición una condición (**Disparos ≥ N**, **Pasaron
   X s** o **Sin enemigos en alcance**) con su valor, y conectar su salida a otro Estado.
4. Como diseñador, quiero ver en el grafo un **error** si falta el Inicio o no está conectado, y un
   **aviso** si una transición no está conectada, si un Estado no se alcanza desde el Inicio, o si
   un Estado que no puede disparar tiene una transición **Disparos ≥ N** (nunca se cumpliría).
5. Como alumno, quiero abrir la escena **Torreta** (menú **Clase14 > Escenas**), darle Play y ver
   la torreta pasar de amarilla a roja a naranja y volver, varias veces, contra una oleada larga.
6. Como alumno, quiero leer `09-CicloTorreta/LEEME.md` y saber qué abrir primero y por qué este
   grafo necesita ser un grafo (tiene un ciclo y caminos que convergen).

## Decisions
- Carpeta nueva `Unity/Assets/Clase14/09-CicloTorreta/` con `Runtime/` (assembly
  `Clase14.CicloTorreta`, referencia `Clase14.Juego`) y `Editor/` (`Clase14.CicloTorreta.Editor`).
  Namespaces `Clase14` y `Clase14.Editor.TurretCycle`. Extensión del grafo: `turretcycle`. Se
  actualizan el mapa de `AGENTS.md` y el recorrido del `README.md` de clase14.
- Transiciones como **salidas del Estado**, como el `ChoiceNode`: opción `Transiciones` (cantidad,
  1–4) y, por cada una, puerto de condición, puerto de valor y salida de flujo. Los nombres de
  puertos van en una clase de constantes, como `DialoguePorts`.
- Tres condiciones. Cuentan **desde que se entró al Estado** (disparos y segundos se reinician al
  cambiar de Estado). Se evalúan en el orden de las salidas y **gana la primera** que se cumple.
- El Estado inicial lo marca un nodo **Inicio** con una salida, igual que en el diálogo.
- Validación en una función pura sobre el grafo (como `DialogueValidation`), usada por
  `OnGraphChanged` y por los tests.
- El importer genera un `TurretCycleAsset` con estados y transiciones **por índice** (como
  `DialogueAsset`). `TurretCycleRunner` es una clase pura sin MonoBehaviour: recibe el tiempo,
  los disparos y si hay enemigos en alcance, y dice el Estado actual. Se testea en Edit Mode.
- `TurretCycle` es un componente nuevo en `09-CicloTorreta/Runtime/` que maneja a la `Turret`:
  la habilita o no para disparar, ajusta su cadencia y la pinta. A `Turret` solo se le agrega lo
  mínimo para eso (por ejemplo un evento al disparar y la forma de saber si hay enemigos en
  alcance); `Main` y `Profiler` no cambian de comportamiento.
- `SampleContent` genera el ciclo de ejemplo (`Data/CiclosTorreta/Sobrecalentamiento.turretcycle`)
  y la escena **Torreta** (objetivo, una torreta con `TurretCycle`, una oleada larga), y la agrega
  al build y al menú **Clase14 > Escenas**.
- Fuera de alcance: resaltar el Estado activo en el grafo durante Play Mode
  (`GraphVisualization`); subgrafos; más condiciones.

## Acceptance
- Tests de Edit Mode nuevos: el ciclo de ejemplo no tiene errores ni avisos; cada regla de
  validación se dispara en un grafo armado en código; el importer compila el ciclo de ejemplo a
  los tres estados con sus transiciones; `TurretCycleRunner` recorre Activa → Recalentada →
  Enfriando → Activa, y sale de Enfriando por **Sin enemigos** antes de los 3 s.
- `./scripts/Test.ps1` y `./scripts/Test.ps1 -Platform PlayMode` pasan.
- `./scripts/Build.ps1` termina bien con las cuatro escenas.
- Captura batch de la escena **Torreta** con la torreta en rojo (Recalentada).
