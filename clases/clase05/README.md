# Clase 05 — Refactor SOLID: de un god-class a composición + interfaces

## Objetivo

Este ejemplo muestra, con un sistema simple de enemigos/combate, cómo un código escrito
sin cuidado por el diseño se puede refactorizar para respetar:

- **SRP (S de SOLID):** una clase, una responsabilidad.
- **Composición sobre herencia:** un árbol de herencia profundo no permite combinar
  capacidades ortogonales (por ejemplo "vuela" + "ataca a distancia"); la composición sí.
- **Interfaces y polimorfismo:** permiten intercambiar comportamiento en tiempo de
  ejecución y hacen testeable la lógica de juego.
- **Separación de dominio e infraestructura:** la lógica de gameplay puede vivir en clases
  C# puras, reutilizables y ejecutables fuera de Unity; Unity queda como un adaptador
  delgado.

## Estructura

- `src/UnityStubs`: simula lo mínimo de la API de Unity (`MonoBehaviour`, `Transform`,
  `GameObject`, `Time.deltaTime`, `Debug.Log`, `Object.Instantiate/Destroy`) para poder
  compilar y correr código "con sabor a Unity" sin el Editor instalado. **No reemplaza a
  Unity real**, solo alcanza para este ejemplo.
- `src/Antes`: el punto de partida. Una clase `Enemy` que mezcla movimiento, vida, ataque
  y detalles de infraestructura, con subclases (`FlyingEnemy`, `ArcherEnemy`,
  `FlyingArcherEnemy`, `BossEnemy`) que muestran los límites de la herencia simple.
- `src/Despues/Despues.Domain`: el resultado del refactor. POCOs (`EnemyStats`),
  interfaces chicas (`IMover`, `IAttacker`, `IDamageable`, `IProjectileSpawner`) y
  `EnemyController` como raíz de composición. Sin ninguna referencia a Unity.
- `src/Despues/Despues.Unity`: adaptador delgado (`EnemyView`) que conecta
  `Despues.Domain` con `UnityStubs`.
- `src/Despues/Despues.DomainConsole`: la misma partida, corrida 100% headless, sin
  ningún tipo de `UnityStubs` — prueba concreta de que separar el dominio de la
  infraestructura permite correr la lógica donde uno quiera.
- `tests/Despues.Tests`: tests xUnit sobre `Despues.Domain` únicamente.

## Cómo correr

```bash
dotnet run --project src/Antes
dotnet run --project src/Despues/Despues.Unity
dotnet run --project src/Despues/Despues.DomainConsole
dotnet test
```

## Guía de pasos sugeridos para el refactor en vivo

1. Mostrar `Antes` corriendo y leer `Enemy.cs`: identificar las distintas razones de
   cambio mezcladas (gameplay, sonido, UI, balance) → disparador de SRP.
2. Mostrar `FlyingEnemy`, `ArcherEnemy` y en particular `FlyingArcherEnemy`: evidenciar la
   duplicación forzada por la herencia simple → disparador de composición vs. herencia.
3. Extraer `EnemyStats` (POCO) desde los campos sueltos de `Enemy`.
4. Extraer `IMover`/`IAttacker` y sus implementaciones a partir del movimiento y el ataque
   ya identificados en el paso 2.
5. Armar `EnemyController` componiendo `EnemyStats` + `IMover` + `IAttacker`.
6. Resolver `FlyingArcherEnemy` con `EnemyFactory.CreateFlyingArcher(...)` — sin clase
   nueva. Punto culminante de la clase.
7. Mostrar el cambio de `IAttacker` en tiempo de ejecución para el jefe (enfurecimiento) →
   disparador de polimorfismo.
8. Escribir en vivo uno de los tests de `Despues.Tests` sobre `EnemyController` → disparador
   de testeabilidad vía interfaces.
9. Correr `Despues.DomainConsole` al lado de `Despues.Unity` → disparador de separación
   dominio/infraestructura.

## Spec

Diseño completo en [`SPEC.md`](SPEC.md).
