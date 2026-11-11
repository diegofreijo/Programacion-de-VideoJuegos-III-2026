# Ideas: un segundo grafo de Graph Toolkit para clase14

Catálogo de ideas para un segundo ejemplo de grafo custom de editor en clase14 (#19). Sale de un
grilling con estas decisiones:

- Es un **ejemplo para mostrar en clase** en **menos de 15 minutos**: se apoya en lo que ya se
  vio con el diálogo (`05-GraphToolkit/`) y suma **un** concepto nuevo de Graph Toolkit.
- **Mismo modelo que el diálogo**: grafo → `ScriptedImporter` → asset plano de runtime. El juego
  nunca ve el grafo.
- **Solo Graph Toolkit** (módulo del Editor desde 6000.4; el proyecto usa 6000.6.5f1).
- Trabaja sobre **los datos del juego de oleadas** (`EnemyDefinition`, `WaveDefinition`,
  `Turret`, `CombatRules`).
- Vive en una **carpeta nueva de clase14**, con `Runtime/`, `Editor/`, asmdefs y `LEEME.md`, y
  tiene **su propia escena**.

Cada idea dice qué modela, qué concepto nuevo muestra, sus nodos, la validación, qué genera el
importer, la escena y un test. Los conceptos se comprobaron en la API del módulo
(`UnityEditor.GraphToolkitModule.xml` de 6000.6.5f1).

## Resumen

| # | Idea | Concepto nuevo | ¿Entra en 15 min? |
| --- | --- | --- | --- |
| 1 | Oleada por bloques | `ContextNode` + `BlockNode` | Sí |
| 2 | Campaña de oleadas | Assets en puertos + dependencias del importer | Sí |
| 3 | Curva de dificultad | Puertos de datos tipados y evaluación | Sí, justo |
| 4 | Árbol de mejoras de torreta | Grafo no lineal (DAG), capacidad múltiple | Justo |
| 5 | Botín con variables | Variables del grafo (Blackboard) | Sí |
| 6 | Patrones reutilizables | Subgrafos | No, mejor como continuación de la 1 o la 2 |
| 7 | Depurar en vivo | `GraphVisualization` (resaltar el nodo activo en Play Mode) | Sí, pero sin escena propia |
| 8 | Botón "Simular" | `GraphToolbarElement` en la ventana del grafo | Sí, como agregado |

## 1. Oleada por bloques

**Modela**: una oleada como lista de pasos ordenados, en vez de la tabla de `WaveDefinition`.

**Concepto nuevo**: `ContextNode` y `BlockNode` (con `[UseWithContext]`): un nodo contenedor donde
se apilan bloques, como en VFX Graph. El orden de los bloques **es** el orden de ejecución, sin
cables.

**Nodos**:
- `WaveContext` (contexto "Oleada"), con la opción radio de aparición.
- Bloques: `SpawnBlock` (enemigo `EnemyDefinition`, cantidad, intervalo), `WaitBlock` (segundos),
  `BossBlock` (un enemigo, sin intervalo).

**Validación** (`OnGraphChanged`): contexto sin bloques; `SpawnBlock` sin enemigo o con un enemigo
que no pasa `EnemyDefinition.Validate()` (se reusa, no se duplica la regla); dos `WaitBlock`
seguidos.

**Importer**: genera un `ScriptedWave` con una lista de pasos `{tipo, enemigo, cantidad,
segundos}` en el orden de los bloques.

**Escena**: `Bloques`, con un spawner que recorre los pasos (reusa `WaveSpawner.Spawn`).

**Test**: un grafo armado en código con `Graph.AddNode` + `ContextNode.AddBlockNode` se compila a
la lista de pasos esperada.

**Por qué**: es lo que más se diferencia visualmente del diálogo y el importer es todavía más
simple (recorrer bloques en orden). Encaja con los 15 minutos.

## 2. Campaña de oleadas

**Modela**: la secuencia de oleadas de una partida, con pausas y una rama según cómo le fue al
jugador.

**Concepto nuevo**: **referencias a assets en puertos** (`AddInputPort<WaveDefinition>`) y
**dependencias del importer**: si se edita una `WaveDefinition`, se reimporta la campaña
(`ctx.DependsOnSourceAsset`). El diálogo solo tenía strings.

**Nodos**: `Inicio`, `Oleada` (una `WaveDefinition`), `Pausa` (segundos), `SiObjetivo` (vida del
objetivo por encima/debajo de un %: dos salidas), `Fin`.

**Validación**: oleada sin asignar o inválida (`WaveDefinition.Validate()`); DPS acumulado de la
campaña por encima de un umbral; ramas sin `Fin`.

**Importer**: `CampaignAsset` con pasos indexados, igual que `DialogueAsset` (mismo recorrido BFS).

**Escena**: `Campaña`, con un `CampaignPlayer` que reemplaza la lista `waves` del `WaveSpawner`.

**Test**: cambiar el `count` de una oleada y comprobar que el `CampaignAsset` reimportado lo
refleja.

**Por qué**: muestra el paso de "el grafo tiene texto" a "el grafo apunta a otros assets", que es
lo que más van a necesitar los alumnos. El importer es casi el del diálogo, así que el tiempo va al
concepto nuevo.

## 3. Curva de dificultad

**Modela**: cuánto escalan la vida y la cantidad de enemigos con el número de oleada.

**Concepto nuevo**: **puertos de datos tipados** (`float`, `AnimationCurve`) y un importer que
**evalúa** el grafo de datos recursivamente, de la salida hacia las entradas.

**Nodos**: `NúmeroDeOleada` (entrada), `Constante`, `Sumar`, `Multiplicar`, `Curva`,
`Salida` (con dos puertos: multiplicador de vida, multiplicador de cantidad).

**Validación**: salida desconectada; multiplicador ≤ 0 en alguna oleada de 1 a N; ciclos (si
`IsConnectionAllowed` no los impide).

**Importer**: evalúa el grafo para las oleadas 1..N y guarda una tabla plana
`float[] healthMultiplier`, `float[] countMultiplier`. En runtime no hay grafo ni evaluación.

**Escena**: `Dificultad`, un gráfico de barras con `WorldText`/sprites que muestra la tabla.

**Test**: evaluación pura: `2 + oleada × 0.5` da `[2.5, 3, 3.5…]`.

**Por qué**: es el otro gran tipo de grafo (de datos, no de flujo), como Shader Graph. Entra justo
en 15 minutos si se dan pocos nodos.

## 4. Árbol de mejoras de torreta

**Modela**: mejoras de la torreta (daño, cadencia, alcance) con costo y prerrequisitos.

**Concepto nuevo**: grafo **no lineal**: entradas con `PortCapacity.Multiple` (una mejora pide
varias anteriores) y orden topológico en el importer.

**Nodos**: `Base` (valores iniciales de `Turret`), `Mejora` (costo, +daño, +disparos/s, +alcance).

**Validación**: ciclos; mejora inalcanzable; DPS final contra el enemigo más resistente medido con
`CombatRules.SecondsToKill`.

**Importer**: `UpgradeTree` con mejoras indexadas y la lista de prerrequisitos de cada una.

**Escena**: `Mejoras`, con botones que compran mejoras con oro (el `reward` de los enemigos).

**Test**: el orden topológico respeta los prerrequisitos; un ciclo produce un error.

**Por qué**: ejemplo clásico de herramienta de diseño. Lo justo de tiempo es por el orden
topológico.

## 5. Botín con variables

**Modela**: qué recompensa da cada enemigo al morir, con probabilidades.

**Concepto nuevo**: **variables del grafo** (Blackboard, `Graph.GetVariables`, `IVariable`): por
ejemplo `MultiplicadorDeOro` definido una vez y leído desde varios nodos.

**Nodos**: `Enemigo` (`EnemyDefinition`), `Aleatorio` (N salidas con peso), `Oro` (cantidad),
`Nada`.

**Validación**: pesos que suman 0; enemigo repetido en dos nodos.

**Importer**: `LootTable` con probabilidades ya normalizadas y las variables ya aplicadas.

**Escena**: `Botín`, matar enemigos con clic y ver el oro.

**Test**: pesos `1, 3` dan probabilidades `0.25, 0.75`; cambiar la variable cambia el oro.

## 6. Patrones reutilizables (subgrafos)

**Modela**: patrones de aparición ("Pinza", "Goteo", "Enjambre") usados desde varias oleadas.

**Concepto nuevo**: **subgrafos** (`[Subgraph]`, `ISubgraphNode.GetSubgraph`) en archivo propio y
locales (Ctrl+Shift+L).

**Por qué no sola**: necesita un grafo base que ya funcione. Va mejor como segunda parte de la 1 o
la 2, o como ejercicio para los alumnos.

## 7. Depurar el diálogo en vivo

**Modela**: nada nuevo: es el diálogo de `05-GraphToolkit/`.

**Concepto nuevo**: `GraphVisualization` (`Context`, `NodeReference`, `GraphMotion.Play`): con el
grafo abierto y el juego en Play Mode, se anima el nodo de la línea que se está mostrando.

**Qué cambia**: el importer guarda, por cada línea, el id del nodo de origen; un script de Editor
escucha al `DialogueCursor` y resalta ese nodo.

**Por qué**: es muy corto y vistoso, y muestra que "el grafo no se ejecuta" no impide depurarlo.
**Choca con una decisión**: no tiene escena propia (usa la escena `Dialogo`).

## 8. Botón "Simular" en la ventana del grafo

**Concepto nuevo**: `GraphToolbarElement` para agregar un botón a la barra de la ventana del
grafo, que compila el grafo y muestra un resumen (duración de la oleada, vida total, DPS) sin
entrar en Play Mode.

**Por qué como agregado**: solo luce sobre otra idea (la 1 o la 2). Suma 2–3 minutos.

## Recomendación

**La 1 (oleada por bloques)**, con la 8 si sobra tiempo: muestra el concepto visualmente más
distinto del diálogo, el importer es el más simple de todos, reusa `Validate()` y
`WaveSpawner.Spawn`, y tiene escena propia. Si lo que importa es lo que más van a necesitar los
alumnos en sus TPs, **la 2 (campaña)**.

## Funciones nuevas que solo entran en un grafo

Un grafo se justifica cuando los datos tienen **ciclos** (se vuelve a un estado anterior),
**convergencia** (varios caminos llegan al mismo nodo) o **ramas con condición**. Si no tienen
ninguna de las tres, una lista o una tabla en el Inspector es mejor. Todas estas son funciones
nuevas y chicas para el juego de oleadas:

| # | Idea | Por qué necesita un grafo |
| --- | --- | --- |
| 9 | **Ciclo de la torreta** (sobrecalentamiento): Activa → Recalentada → Enfriando → Activa | Ciclo, y dos salidas de Enfriando que convergen en Activa. **Elegida: Spec en #20** (`docs/specs/20-clase14-ciclo-torreta.md`). |
| 10 | **Fases del jefe**: Fase 1 → (vida < 60%) Fase 2 → (vida < 30%) Fase 3; desde cualquiera, (más de 20 s) → Furia | Ramas con condición y convergencia en Furia. |
| 11 | **Transformaciones de enemigos**: el Orco muere y aparecen 2 Duendes; el Berserker baja de 50% y pasa a BerserkerElite | Varios orígenes y destinos por enemigo; la validación detecta ciclos (Orco → Duende → Orco). |
| 12 | **Reacciones de estados alterados**: Fuego + Hielo → Vapor; Fuego + Veneno → Explosión | El nodo Reacción tiene dos entradas (muchos a muchos). La más grande: pide crear estados alterados. |
| 13 | **Eventos encadenados del objetivo**: vida < 50% → refuerzos → 3 s → sobrecarga de torretas | Ramas por umbral que convergen en la misma acción. Casi el `DialogueAsset` con otros nodos. |
| 14 | **Rutas con bifurcaciones** (waypoints) | Convergencia y ramas con peso, pero las posiciones se editan mejor en la escena: sirve de **contraejemplo**, cuando el grafo de nodos es la herramienta equivocada. |
