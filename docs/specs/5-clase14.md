**Tier:** Job · **Tickets:** 1 · **Pause points:** none · **Pick:** none · **Review:** none · **Landing:** Merged PR

## Problem
La clase 14 muestra herramientas de Editor en Unity y cierra con una demo de un agente de código.
Hace falta un proyecto chico, ya armado, sobre el que mostrar cada herramienta y que un agente
pueda modificar y verificar solo.

## Solution
Un proyecto `clases/clase14` (Unity 6000.6.5f1): un juego 2D top-down de oleadas con datos en
ScriptableObjects, las herramientas de Editor del bloque 1, un editor de diálogos por nodos con
Graph Toolkit, tests y scripts batch, y un `AGENTS.md` de ejemplo.

## Behaviours
1. Como alumno, quiero abrir la escena principal y ver oleadas de enemigos atacando un objetivo que una torreta defiende, para tener un juego concreto detrás de las herramientas.
2. Como diseñador, quiero editar enemigos y oleadas como assets, con atributos y `OnValidate` que corrigen datos, para no depender del programador.
3. Como diseñador, quiero editar el intervalo de aparición con un slider min–max (PropertyDrawer propio), en UI Toolkit e IMGUI.
4. Como diseñador, quiero que el Inspector de un enemigo y de una oleada muestre una vista previa calculada (DPS, duración, vida total), las validaciones y botones de acción.
5. Como diseñador, quiero una ventana de balanceo con todos los enemigos en una tabla editable, con los datos inválidos marcados, y crear enemigos desde un menú.
6. Como diseñador, quiero armar diálogos con nodos (Inicio, Línea, Elección, Fin) con validaciones visibles en el grafo, y que el juego use un asset de runtime generado al importar.
7. Como alumno o agente, quiero correr los tests de Edit y Play Mode y el build desde la terminal sin abrir el Editor.
8. Como alumno, quiero una escena que genere picos de GC visibles en el Profiler.
9. Como alumno, quiero un `AGENTS.md` de ejemplo que explique el proyecto, los comandos y las reglas a un agente.

## Decisions
- Las reglas de validación de datos viven en un único `Validate()` por ScriptableObject; Inspector, ventana y tests lo reutilizan. `OnValidate` solo corrige.
- Las reglas de combate son funciones puras, testeables en Edit Mode sin escena; ahí se apoya la demo de agente (agregar una regla con su test).
- Toda la UI de Editor es UI Toolkit; el PropertyDrawer incluye además su versión IMGUI para comparar.
- Graph Toolkit solo para autoría: un ScriptedImporter compila el grafo a un ScriptableObject de runtime; el assembly de runtime no referencia el módulo de Editor.
- Datos, diálogo y escenas de ejemplo se generan por código (un método de Editor) y se versionan; dos enemigos son inválidos a propósito.
- Sin arte ni paquetes extra: sprites generados por código, Built-in Render Pipeline, sin Input System.
- Las dos tareas de la demo de agente quedan sin hacer en la rama principal.

## Testing
- Edit Mode: reglas de datos de enemigo y oleada, reglas de combate, validación y compilación del grafo de diálogo (incluido el importer), y los assets reales del proyecto. Prior art: clase08.
- Play Mode: el spawner crea toda la oleada, un enemigo llega y golpea al objetivo, la torreta mata a un enemigo.

## Acceptance criteria
- [ ] `scripts/Test.ps1` pasa (Edit Mode).
- [ ] `scripts/Test.ps1 -Platform PlayMode` pasa.
- [ ] `scripts/Build.ps1` produce el build de Windows.
- [ ] El proyecto compila sin advertencias.
- [ ] El Inspector, la ventana de balanceo y el editor de diálogos se ven correctamente (capturas en batch).
- [ ] README de la clase y `AGENTS.md` de ejemplo.

## Out of scope
- Gizmos y Handles, Frame Debugger.
- Las dos tareas que hace el agente en vivo.
