# 09 · Ciclo de la torreta: un grafo que necesita ser grafo

El diálogo de `05-GraphToolkit` es casi una lista con ramas. Este ejemplo muestra datos que **solo**
se pueden escribir como grafo: el ciclo de sobrecalentamiento de la torreta.

```
        ┌── Disparos ≥ 10 ──▶ [Recalentada]
[Activa]                         │ Pasaron 2 s
   ▲                             ▼
   └─ Pasaron 3 s / Sin enemigos ─ [Enfriando]
```

- Tiene un **ciclo**: Activa → Recalentada → Enfriando → Activa, sin fin. Una lista termina.
- Tiene **caminos que convergen**: las dos salidas de Enfriando llegan a Activa. Un árbol no puede.

Mismo modelo que el diálogo: grafo → `ScriptedImporter` → asset plano de runtime. El grafo nunca se
ejecuta.

1. `Editor/TurretCycleGraph.cs` y `TurretCycleNodes.cs`: el grafo y sus nodos **Inicio** y
   **Estado**. Cada transición es una salida del Estado (condición, valor y a qué Estado lleva), como
   las opciones del `ChoiceNode`; la opción **Transiciones** dice cuántas hay. La condición es un
   enum (`TurretCondition`): Graph Toolkit lo muestra como desplegable.
2. `Editor/TurretCycleValidation.cs`: errores (falta el Inicio o no está conectado) y avisos
   (transición sin conectar, Estado inalcanzable, Estado que no dispara con **Disparos ≥ N**).
3. `Editor/TurretCycleImporter.cs`: al guardar, numera los Estados y enlaza las transiciones por
   índice en un `TurretCycleAsset`. Con ciclos, cada nodo recibe su índice una sola vez.
4. `Runtime/TurretCycleRunner.cs`: clase pura que recorre el asset. Las condiciones cuentan desde
   que se entró al Estado, se evalúan en orden y gana la primera que se cumple.
5. `Runtime/TurretCycle.cs`: el componente que maneja a la `Turret` (si dispara, su cadencia y su
   color).

Para verlo: doble clic en `Data/CiclosTorreta/Sobrecalentamiento.turretcycle`, y Play en la escena
**Torreta** (menú **Clase14 > Escenas**): la torreta pasa de amarilla a roja a naranja y vuelve.
Probá cambiar **Disparos ≥ 10** por 3, guardar y darle Play de nuevo. Para crear otro ciclo:
**Assets > Create > Clase14 > Ciclo de torreta**.
