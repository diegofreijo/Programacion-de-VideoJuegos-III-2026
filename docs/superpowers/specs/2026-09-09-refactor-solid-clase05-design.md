# Clase 05 — Ejemplo didáctico de refactor en C# (SRP, composición vs. herencia, interfaces, separación dominio/infraestructura)

## 1. Objetivo pedagógico

Dar un ejemplo compilable y ejecutable (sin necesidad de abrir Unity) de un sistema de
gameplay escrito sin cuidado por el diseño, y su refactor a un diseño que respeta:

- **SRP (S de SOLID):** una clase, una responsabilidad / una razón para cambiar.
- **Composición sobre herencia:** un árbol de herencia profundo no permite combinar
  capacidades ortogonales (ej. "vuela" + "ataca a distancia"); la composición sí.
- **Interfaces:** permiten intercambiar comportamiento, habilitan polimorfismo real
  (mismo código, distinto comportamiento según el objeto) y hacen testeable la lógica.
- **Separación de datos/dominio vs. infraestructura (Unity):** la lógica de gameplay
  puede vivir en clases C# puras, reutilizables y ejecutables fuera de Unity; Unity queda
  como un adaptador delgado.

La clase dura 3 horas. Este material no se pre-hornea commit a commit: se entrega un
punto de partida (`Antes`) y un punto de llegada (`Despues`) ya funcionando, y el profesor
hace el refactor en vivo apoyándose en la guía de pasos de la sección 7.

## 2. Formato y alcance

- Solución **C# pura** (`.sln` + proyectos `.csproj`), sin dependencia del Editor de Unity.
  Se ejecuta con `dotnet run` / `dotnet test`.
- Una capa `UnityStubs` compartida simula lo mínimo de la API de Unity (`MonoBehaviour`,
  `Transform`, `GameObject`, `Time.deltaTime`, `Debug.Log`, `Object.Instantiate`/`Destroy`)
  para que el código de `Antes` y de la parte Unity de `Despues` use la sintaxis real de
  Unity (`Update()`, `transform.position`, etc.) sin necesitar el motor instalado. Se aclara
  a los alumnos que esto **simula** Unity, no lo reemplaza.
- Dominio: un sistema simple de enemigos/combate (persigue y ataca al jugador).

## 3. Estructura del repositorio

```
clase05/
  README.md                         # guía de la clase (objetivos, cómo correr, pasos del refactor en vivo)
  RefactorSolidComposicion.sln
  src/
    UnityStubs/                     # compartido, NO es parte de la lección en sí
      Vector3.cs
      Transform.cs
      GameObject.cs
      MonoBehaviour.cs
      Time.cs
      Debug.cs
      UnityObject.cs                # Instantiate / Destroy estáticos
    Antes/
      Program.cs                    # arma jugador + varios enemigos, simula N frames por consola
      Player.cs
      Enemy.cs
      FlyingEnemy.cs
      ArcherEnemy.cs
      FlyingArcherEnemy.cs
      BossEnemy.cs
      Projectile.cs
    Despues/
      Despues.Domain/               # C# puro. CERO referencia a UnityStubs.
        Vector3D.cs                 # vector propio del dominio, independiente del de UnityStubs
        EnemyStats.cs
        IDamageable.cs
        IMover.cs
        GroundMover.cs
        FlyingMover.cs
        IAttacker.cs
        MeleeAttack.cs
        RangedAttack.cs
        IProjectileSpawner.cs
        EnemyController.cs
        PlayerCharacter.cs
        EnemyFactory.cs
      Despues.Unity/                # referencia Despues.Domain + UnityStubs
        EnemyView.cs                 # MonoBehaviour delgado
        UnityProjectileSpawner.cs
        Program.cs                   # misma escena que Antes/Program.cs, corrida "como si" fuera Unity
      Despues.DomainConsole/        # referencia SOLO Despues.Domain. Nada de Unity.
        ConsoleProjectileSpawner.cs
        Program.cs                   # misma partida, 100% headless
  tests/
    Despues.Tests/                  # referencia SOLO Despues.Domain (xUnit)
      EnemyControllerTests.cs
      RangedAttackTests.cs
```

## 4. `Antes` — el código a refactorizar

Objetivo: que la mala práctica duela de forma concreta, no solo declarada.

- **`Enemy : MonoBehaviour`** concentra: datos (vida, velocidad, daño, rango, cooldown como
  campos sueltos, sin POCO), movimiento hacia el jugador, chequeo de cooldown de ataque,
  chequeo de muerte, y detalles de infraestructura (`Debug.Log`, "actualizar UI de vida",
  destrucción del `GameObject`) — todo mezclado dentro de `Update()` y `TakeDamage()`.
  `Attack()` es `virtual`, con implementación melee en la base.

  El `README` y un comentario en el archivo listan explícitamente las **distintas razones
  para cambiar** que conviven en esta única clase: el programador de gameplay (fórmula de
  daño), el de sonido (efecto al morir/atacar), el de UI (barra de vida) y el diseñador
  (balance de stats). Cualquiera de ellos toca el mismo método. Y no hay forma de testear
  "¿`TakeDamage` resta bien la vida?" sin arrastrar todo lo demás.

- **`FlyingEnemy : Enemy`** — tiene que overridear `Update()` casi completo solo para
  cambiar cómo se mueve, porque el movimiento no está aislado del resto de la lógica.

- **`ArcherEnemy : Enemy`** — overridea `Attack()` para instanciar un `Projectile` en vez
  de pegar cuerpo a cuerpo, duplicando el chequeo de rango/cooldown que vive en `Update()`.

- **`FlyingArcherEnemy : FlyingEnemy`** — necesita volar **y** atacar a distancia a la vez.
  Con herencia simple hay que elegir de cuál heredar (`FlyingEnemy`) y **copiar/pegar** el
  `Attack()` de `ArcherEnemy` a mano, con un comentario que lo señala explícitamente. Este
  es el punto fuerte para mostrar en vivo: no hay forma prolija de resolver esto con
  herencia en C#.

- **`BossEnemy : FlyingArcherEnemy`** — agrega una fase de "enfurecimiento" con un
  `if (Health < MaxHealth * 0.3f)` embebido en `Attack()`/`Update()` que cambia el
  comportamiento a mano, duplicando aún más lógica.

- **`Projectile : MonoBehaviour`** y **`Player : MonoBehaviour`** — clases mínimas de apoyo
  para que la escena tenga sentido (el player recibe daño y tiene vida).

- **`Program.cs`** — el "motor": crea un `Player` y un enemigo de cada subtipo, corre N
  frames simulados seteando `Time.deltaTime` y llamando `Update()` en cada uno, imprimiendo
  el estado por consola.

## 5. `Despues` — el diseño refactorizado

### 5.1. `Despues.Domain` (sin dependencia de Unity ni de `UnityStubs`)

- **`Vector3D`**: struct propio del dominio (X, Y, Z, distancia, `MoveTowards`).
  Deliberadamente distinto del `Vector3` de `UnityStubs` — el dominio no conoce el vector
  del motor; la capa `Despues.Unity` es la que convierte entre uno y otro. Este es otro
  ejemplo chico pero concreto de "el dominio no depende de la infraestructura".
- **`EnemyStats`** (POCO): nombre, vida, vida máxima, velocidad, daño, rango, cooldown.
- **`IDamageable`**: `Health`, `IsDead`, `TakeDamage(amount)`.
- **`IMover`**: mueve una posición hacia un objetivo. Implementaciones: `GroundMover`,
  `FlyingMover`.
- **`IAttacker`**: intenta atacar a un `IDamageable` dado cooldown/rango; expone un
  `Name` legible ("Cuerpo a cuerpo", "A distancia") para poder mostrar por consola cuándo
  cambia. Implementaciones: `MeleeAttack`, `RangedAttack(IProjectileSpawner)`.
- **`IProjectileSpawner`**: puerto para lanzar un proyectil (posición origen/destino, daño).
  Cada entorno de ejecución trae su propia implementación (ver 5.2 y 5.3).
- **`EnemyController`** (clase común, no `MonoBehaviour`): implementa `IDamageable`;
  compone `EnemyStats` + posición (`Vector3D`) + un `IMover` + un `IAttacker`. Expone
  `Tick(targetPosition, target, deltaTime)` y `TakeDamage(amount)`. Soporta reasignar su
  `IAttacker` en tiempo de ejecución (usado por el "enfurecimiento" del jefe, ver abajo) y
  opcionalmente un umbral de vida + un `IAttacker` de repuesto para hacer ese cambio solo
  automáticamente — así el jefe no necesita una clase ni un `if` por fuera: es una
  composición con una regla de transición incluida.
- **`PlayerCharacter`**: implementación mínima de `IDamageable` para el objetivo de los
  enemigos, reutilizada igual en Unity y en consola pura.
- **`EnemyFactory`**: `CreateGoblin`, `CreateSkeletonArcher`, `CreateBat`,
  `CreateFlyingArcher`, `CreateBoss`. La combinación "vuela + ataca a distancia" que era
  imposible de expresar limpiamente con herencia en `Antes` acá es una sola llamada a
  `EnemyFactory.CreateFlyingArcher(...)`, sin ninguna clase nueva.

**Polimorfismo, explícito en la demo:** el loop principal (en ambos `Program.cs` de
`Despues`) itera una lista de `EnemyController` con distintas combinaciones de
`IMover`/`IAttacker` y llama `Tick()` sin saber de qué tipo concreto es cada estrategia —
mismo código, comportamiento distinto según el objeto. El cambio de `IAttacker` del jefe al
entrar en fase de enfurecimiento es el contraste directo con el `if (Health < ...)` de
`Antes`: acá es una reasignación de estrategia, sin condicionales nuevos ni herencia.

### 5.2. `Despues.Unity` (referencia `Despues.Domain` + `UnityStubs`)

- **`EnemyView : MonoBehaviour`**: adaptador delgado. Contiene un `EnemyController`;
  en `Update()` convierte `transform.position` (`UnityStubs.Vector3`) a `Vector3D`, llama
  `controller.Tick(...)` y aplica la posición resultante de vuelta al `transform`.
- **`UnityProjectileSpawner : IProjectileSpawner`**: implementa el spawn usando
  `UnityObject.Instantiate` y `Debug.Log`.
- **`Program.cs`**: misma escena que `Antes/Program.cs` (mismo jugador, mismos tipos de
  enemigo, mismo número de frames) pero construida sobre `EnemyFactory` + `EnemyView`. Al
  ponerlos lado a lado, el contraste de acoplamiento entre ambos `Program.cs` es en sí
  mismo material didáctico.

### 5.3. `Despues.DomainConsole` (referencia SOLO `Despues.Domain`)

- **`ConsoleProjectileSpawner : IProjectileSpawner`**: implementación mínima que solo
  escribe por consola (`Console.WriteLine`), sin ningún tipo de `UnityStubs`.
- **`Program.cs`**: arma la misma partida (mismos `EnemyController` vía `EnemyFactory`, un
  `PlayerCharacter`) y corre el mismo número de "frames" a mano, sin `GameObject`,
  `Transform` ni `MonoBehaviour` en absoluto. Prueba concreta de que la lógica de dominio
  corre en cualquier lado (útil, por ejemplo, para una herramienta de balance o una
  simulación en servidor) una vez separada de Unity.

### 5.4. `tests/Despues.Tests` (xUnit, referencia SOLO `Despues.Domain`)

- **`EnemyControllerTests`**: `TakeDamage` resta vida correctamente; al llegar a 0,
  `IsDead` pasa a `true`.
- **`RangedAttackTests`**: con un `IProjectileSpawner` de prueba (test double que registra
  llamadas), verifica que se dispara un proyectil solo cuando el objetivo está en rango y
  ya pasó el cooldown.

No se busca cobertura exhaustiva — un par de tests simples que prueben el punto ("esto
ahora se puede testear sin Unity") alcanza.

## 6. `README.md` de `clase05`

Contenido:
- Objetivo pedagógico (resumen de la sección 1).
- Cómo correr cada pieza: `dotnet run --project src/Antes`,
  `dotnet run --project src/Despues/Despues.Unity`,
  `dotnet run --project src/Despues/Despues.DomainConsole`, `dotnet test`.
- Aclaración de que `UnityStubs` simula Unity y no lo reemplaza.
- Guía de pasos sugeridos para el refactor en vivo (sección 7).

## 7. Guía de pasos sugeridos para el refactor en vivo

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

## 8. Fuera de alcance

- Sin física real, colisiones, ni renderizado — todo simulado/impreso por consola.
- Sin ScriptableObjects, sin persistencia/guardado.
- Sin cobertura de tests exhaustiva ni mocking framework — se usan test doubles simples
  escritos a mano.
- `UnityStubs` no busca ser fiel a la API real de Unity más allá de lo que este ejemplo
  necesita.
