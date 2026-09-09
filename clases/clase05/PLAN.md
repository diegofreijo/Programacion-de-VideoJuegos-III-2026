# Ejemplo Didáctico de Refactor SOLID (Clase 05) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a compilable, runnable-without-Unity C# example (`Antes`/`Despues`) that demonstrates SRP, composición vs. herencia, interfaces/polimorfismo y separación dominio/infraestructura, for use live in a 3-hour class.

**Architecture:** A shared `UnityStubs` class library simulates the minimal Unity API (`MonoBehaviour`, `Transform`, `GameObject`, `Time`, `Debug`, `Object.Instantiate/Destroy`) so `Antes` (a single tangled project) and `Despues.Unity` (a thin adapter) can use real Unity-flavored syntax. `Despues.Domain` is a pure C# class library with zero reference to `UnityStubs`, containing POCOs, small interfaces (`IMover`, `IAttacker`, `IDamageable`, `IProjectileSpawner`) and `EnemyController` as the composition root. `Despues.DomainConsole` proves the domain runs headless. `Despues.Tests` is an xUnit project that only references `Despues.Domain`.

**Tech Stack:** .NET 9 SDK, C# (`dotnet new`/`dotnet build`/`dotnet run`/`dotnet test`), xUnit.

**Spec:** [SPEC.md](SPEC.md)

## Global Constraints

- All paths in this plan are relative to `clase05/`, i.e.
  `/Users/giga/code/Programacion-de-VideoJuegos-III-2026/clases/clase05`. Run every command
  from that directory unless a step says otherwise.
- Target Framework: `net9.0` for every project (this machine has the .NET 9 SDK/runtime
  only — confirmed via `dotnet --list-runtimes`).
- `Despues.Domain` must never reference `UnityStubs` or any other project — it is pure C#.
- `Despues.Tests` references only `Despues.Domain`.
- Tests use xUnit (`dotnet new xunit`), hand-written test doubles — no mocking framework.
- Console/log messages are in Spanish, matching the rest of the course material.
- No physics, rendering, ScriptableObjects, or persistence — everything is simulated by
  printing to the console (per spec section 8, "Fuera de alcance").
- `UnityStubs` intentionally does not aim for full fidelity with real `UnityEngine` — only
  what this example needs (documented in the class README, Task 9).

---

### Task 1: Scaffold the solution and `UnityStubs`

**Files:**
- Create: `RefactorSolidComposicion.sln`
- Create: `.gitignore`
- Create: `src/UnityStubs/UnityStubs.csproj` (via `dotnet new`)
- Create: `src/UnityStubs/Vector3.cs`
- Create: `src/UnityStubs/Transform.cs`
- Create: `src/UnityStubs/GameObject.cs`
- Create: `src/UnityStubs/MonoBehaviour.cs`
- Create: `src/UnityStubs/Time.cs`
- Create: `src/UnityStubs/Debug.cs`
- Create: `src/UnityStubs/UnityObject.cs`
- Delete: `src/UnityStubs/Class1.cs` (template default)

**Interfaces:**
- Produces: `UnityStubs.Vector3` (fields `x,y,z`; `static float Distance(Vector3,Vector3)`;
  `static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)`),
  `UnityStubs.Transform` (field `position`), `UnityStubs.GameObject` (fields `name`,
  `transform`, `isDestroyed`), `UnityStubs.MonoBehaviour` (properties `gameObject`,
  `transform`; `virtual void Start()`; `virtual void Update()`),
  `UnityStubs.Time.deltaTime` (static, settable), `UnityStubs.Debug.Log(string)`,
  `UnityStubs.UnityObject.Instantiate<T>(string name, Vector3 position) where T :
  MonoBehaviour, new()`, `UnityStubs.UnityObject.Destroy(GameObject gameObject)`.

This task has no automated tests (per spec, testability is a property of `Despues.Domain`,
not of the Unity simulation layer). Its test cycle is "it builds and a tiny throwaway
smoke check runs", verified in the last step.

- [ ] **Step 1: Create the solution file**

```bash
dotnet new sln -n RefactorSolidComposicion
```

- [ ] **Step 2: Add a .gitignore for .NET build output**

Create `.gitignore`:

```gitignore
bin/
obj/
.vs/
```

- [ ] **Step 3: Scaffold the `UnityStubs` class library and add it to the solution**

```bash
dotnet new classlib -n UnityStubs -o src/UnityStubs --framework net9.0
rm src/UnityStubs/Class1.cs
dotnet sln add src/UnityStubs/UnityStubs.csproj
```

- [ ] **Step 4: Write `Vector3.cs`**

```csharp
namespace UnityStubs;

public struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        var dz = a.z - b.z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        var dx = target.x - current.x;
        var dy = target.y - current.y;
        var dz = target.z - current.z;
        var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance <= maxDistanceDelta || distance == 0f)
        {
            return target;
        }

        var t = maxDistanceDelta / distance;
        return new Vector3(current.x + dx * t, current.y + dy * t, current.z + dz * t);
    }

    public override string ToString() => $"({x:0.0}, {y:0.0}, {z:0.0})";
}
```

- [ ] **Step 5: Write `Transform.cs`**

```csharp
namespace UnityStubs;

public class Transform
{
    public Vector3 position;

    public Transform(Vector3 position)
    {
        this.position = position;
    }
}
```

- [ ] **Step 6: Write `GameObject.cs`**

```csharp
namespace UnityStubs;

public class GameObject
{
    public string name;
    public Transform transform { get; }
    public bool isDestroyed;

    public GameObject(string name, Vector3 position = default)
    {
        this.name = name;
        transform = new Transform(position);
    }
}
```

- [ ] **Step 7: Write `MonoBehaviour.cs`**

```csharp
namespace UnityStubs;

public abstract class MonoBehaviour
{
    public GameObject gameObject { get; internal set; } = null!;

    public Transform transform => gameObject.transform;

    public virtual void Start() { }

    public virtual void Update() { }
}
```

- [ ] **Step 8: Write `Time.cs` and `Debug.cs`**

```csharp
namespace UnityStubs;

public static class Time
{
    public static float deltaTime { get; set; }
}
```

```csharp
namespace UnityStubs;

public static class Debug
{
    public static void Log(string message) => Console.WriteLine(message);
}
```

- [ ] **Step 9: Write `UnityObject.cs`**

```csharp
namespace UnityStubs;

public static class UnityObject
{
    public static T Instantiate<T>(string name, Vector3 position) where T : MonoBehaviour, new()
    {
        var gameObject = new GameObject(name, position);
        var component = new T { gameObject = gameObject };
        component.Start();
        return component;
    }

    public static void Destroy(GameObject gameObject)
    {
        gameObject.isDestroyed = true;
    }
}
```

- [ ] **Step 10: Build to verify it compiles**

```bash
dotnet build src/UnityStubs/UnityStubs.csproj
```

Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 11: Commit**

```bash
git add RefactorSolidComposicion.sln .gitignore src/UnityStubs
git commit -m "Agregar solución y UnityStubs (simulación mínima de la API de Unity)"
```

---

### Task 2: `Antes` — la clase que hace de todo (`Enemy`, `Player`, `Program`)

**Files:**
- Create: `src/Antes/Antes.csproj` (via `dotnet new`)
- Create: `src/Antes/Player.cs`
- Create: `src/Antes/Enemy.cs`
- Create: `src/Antes/Program.cs`

**Interfaces:**
- Consumes: `UnityStubs.MonoBehaviour`, `UnityStubs.Vector3`, `UnityStubs.Time`,
  `UnityStubs.Debug`, `UnityStubs.UnityObject` (Task 1).
- Produces: `Antes.Player` (fields `health`, `maxHealth`; `void TakeDamage(int amount)`),
  `Antes.Enemy` (fields `enemyName`, `health`, `maxHealth`, `moveSpeed`, `attackDamage`,
  `attackRange`, `attackCooldown`; `protected float cooldownTimer`; `protected Player
  target`; `void SetTarget(Player player)`; `override void Update()`; `protected virtual
  void Attack(Player targetPlayer)`; `public virtual void TakeDamage(int amount)`;
  `protected virtual void Die()`) — used as the base class by Task 3.

No automated tests: this class is deliberately hard to test in isolation (that is the
point of the exercise). Verification is "it builds and produces sane console output".

- [ ] **Step 1: Scaffold the `Antes` console project**

```bash
dotnet new console -n Antes -o src/Antes --framework net9.0
dotnet sln add src/Antes/Antes.csproj
dotnet add src/Antes/Antes.csproj reference src/UnityStubs/UnityStubs.csproj
```

- [ ] **Step 2: Write `Player.cs`**

```csharp
using UnityStubs;

namespace Antes;

public class Player : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0)
        {
            health = 0;
        }

        Debug.Log($"[Jugador] recibe {amount} de daño. Vida: {health}/{maxHealth}");
    }
}
```

- [ ] **Step 3: Write `Enemy.cs`**

```csharp
using UnityStubs;

namespace Antes;

// OJO: esta clase mezcla varias razones para cambiar en un solo lugar:
// - el programador de gameplay (fórmula de daño, rango, cooldown)
// - el de sonido/feedback (qué se loguea al atacar o morir)
// - el de UI (la "barra de vida")
// - el diseñador (balance de stats)
// Cualquiera de ellos termina tocando este mismo archivo.
public class Enemy : MonoBehaviour
{
    public string enemyName = "Enemigo";
    public int health = 30;
    public int maxHealth = 30;
    public float moveSpeed = 2f;
    public int attackDamage = 5;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    protected float cooldownTimer;
    protected Player target = null!;

    public void SetTarget(Player player) => target = player;

    public override void Update()
    {
        if (health <= 0 || gameObject.isDestroyed)
        {
            return;
        }

        var distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Attack(target);
            cooldownTimer = attackCooldown;
        }
    }

    protected virtual void Attack(Player targetPlayer)
    {
        Debug.Log($"[{enemyName}] ataca cuerpo a cuerpo por {attackDamage}");
        targetPlayer.TakeDamage(attackDamage);
    }

    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[{enemyName}] recibe {amount} de daño. Vida: {health}/{maxHealth}");
        UpdateHealthBarUI();

        if (health <= 0)
        {
            Die();
        }
    }

    protected void UpdateHealthBarUI()
    {
        // simula actualizar una barra de vida en pantalla
        Debug.Log($"[UI] Barra de vida de {enemyName}: {health}/{maxHealth}");
    }

    protected virtual void Die()
    {
        Debug.Log($"[{enemyName}] murió");
        UnityObject.Destroy(gameObject);
    }
}
```

- [ ] **Step 4: Write `Program.cs`**

```csharp
using UnityStubs;

namespace Antes;

public static class Program
{
    public static void Main()
    {
        var player = UnityObject.Instantiate<Player>("Jugador", new Vector3(0, 0, 0));

        var enemies = new List<Enemy>
        {
            UnityObject.Instantiate<Enemy>("Goblin", new Vector3(-5, 0, 0)),
        };

        foreach (var enemy in enemies)
        {
            enemy.SetTarget(player);
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Time.deltaTime = deltaTime;
            Debug.Log($"--- Frame {frame} ---");

            foreach (var enemy in enemies)
            {
                enemy.Update();
            }

            if (player.health <= 0)
            {
                Debug.Log("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
```

- [ ] **Step 5: Run and verify sane output**

```bash
dotnet run --project src/Antes
```

Expected: 20 frames printed, the goblin approaches and attacks the player periodically,
no exceptions thrown.

- [ ] **Step 6: Commit**

```bash
git add src/Antes
git commit -m "Antes: Enemy como god-class (SRP violado) atacando al jugador"
```

---

### Task 3: `Antes` — el zoo de subclases (explosión de herencia)

**Files:**
- Create: `src/Antes/Projectile.cs`
- Create: `src/Antes/FlyingEnemy.cs`
- Create: `src/Antes/ArcherEnemy.cs`
- Create: `src/Antes/FlyingArcherEnemy.cs`
- Create: `src/Antes/BossEnemy.cs`
- Modify: `src/Antes/Program.cs`

**Interfaces:**
- Consumes: `Antes.Enemy`, `Antes.Player` (Task 2); `UnityStubs.*` (Task 1).
- Produces: `Antes.Projectile` (`void Launch(Player target, int damage)`),
  `Antes.FlyingEnemy : Enemy` (field `flightHeight`), `Antes.ArcherEnemy : Enemy`,
  `Antes.FlyingArcherEnemy : FlyingEnemy`, `Antes.BossEnemy : FlyingArcherEnemy`.

Still no automated tests — same reasoning as Task 2. Verification is running the demo.

- [ ] **Step 1: Write `Projectile.cs`**

```csharp
using UnityStubs;

namespace Antes;

public class Projectile : MonoBehaviour
{
    public void Launch(Player target, int damage)
    {
        Debug.Log($"[Proyectil] vuela hacia {target.gameObject.name} ({damage} de daño)");
        target.TakeDamage(damage);
        UnityObject.Destroy(gameObject);
    }
}
```

- [ ] **Step 2: Write `FlyingEnemy.cs`**

```csharp
using UnityStubs;

namespace Antes;

public class FlyingEnemy : Enemy
{
    public float flightHeight = 3f;

    // Tenemos que reescribir Update() entero solo para cambiar el punto
    // hacia el que se mueve (vuela por encima del jugador). El resto
    // (ataque, cooldown, muerte) queda duplicado tal cual estaba en Enemy.
    public override void Update()
    {
        if (health <= 0 || gameObject.isDestroyed)
        {
            return;
        }

        var hoverPoint = new Vector3(target.transform.position.x, target.transform.position.y + flightHeight, target.transform.position.z);
        var distance = Vector3.Distance(transform.position, hoverPoint);

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverPoint, moveSpeed * Time.deltaTime);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Attack(target);
            cooldownTimer = attackCooldown;
        }
    }
}
```

- [ ] **Step 3: Write `ArcherEnemy.cs`**

```csharp
using UnityStubs;

namespace Antes;

public class ArcherEnemy : Enemy
{
    public ArcherEnemy()
    {
        attackRange = 8f;
        attackDamage = 3;
    }

    protected override void Attack(Player targetPlayer)
    {
        Debug.Log($"[{enemyName}] dispara una flecha por {attackDamage}");
        var projectile = UnityObject.Instantiate<Projectile>("Flecha", transform.position);
        projectile.Launch(targetPlayer, attackDamage);
    }
}
```

- [ ] **Step 4: Write `FlyingArcherEnemy.cs`**

```csharp
using UnityStubs;

namespace Antes;

// No podemos heredar de FlyingEnemy Y de ArcherEnemy al mismo tiempo: C# no
// permite herencia múltiple de clases. Elegimos FlyingEnemy como base (porque
// necesitamos su Update() con el vuelo) y COPIAMOS a mano el Attack() de
// ArcherEnemy acá abajo para tener ambas capacidades.
public class FlyingArcherEnemy : FlyingEnemy
{
    public FlyingArcherEnemy()
    {
        attackRange = 8f;
        attackDamage = 3;
    }

    // Duplicado de ArcherEnemy.Attack()
    protected override void Attack(Player targetPlayer)
    {
        Debug.Log($"[{enemyName}] dispara una flecha por {attackDamage}");
        var projectile = UnityObject.Instantiate<Projectile>("Flecha", transform.position);
        projectile.Launch(targetPlayer, attackDamage);
    }
}
```

- [ ] **Step 5: Write `BossEnemy.cs`**

```csharp
using UnityStubs;

namespace Antes;

public class BossEnemy : FlyingArcherEnemy
{
    private const float EnragedHealthFraction = 0.3f;

    public BossEnemy()
    {
        maxHealth = 120;
        health = 120;
        attackDamage = 4;
    }

    // La fase de enfurecimiento se resuelve con un if sobre la vida actual,
    // repitiendo a mano la lógica de ataque cuerpo a cuerpo de Enemy.Attack()
    // porque tampoco podemos heredar de esa versión y de esta al mismo tiempo.
    protected override void Attack(Player targetPlayer)
    {
        var isEnraged = health <= maxHealth * EnragedHealthFraction;

        if (isEnraged)
        {
            Debug.Log($"[{enemyName}] (¡ENFURECIDO!) ataca cuerpo a cuerpo por {attackDamage * 2}");
            targetPlayer.TakeDamage(attackDamage * 2);
            return;
        }

        Debug.Log($"[{enemyName}] dispara una flecha por {attackDamage}");
        var projectile = UnityObject.Instantiate<Projectile>("Flecha del jefe", transform.position);
        projectile.Launch(targetPlayer, attackDamage);
    }
}
```

- [ ] **Step 6: Update `Program.cs` to spawn one of each enemy type**

Replace the `enemies` list body:

```csharp
        var enemies = new List<Enemy>
        {
            UnityObject.Instantiate<Enemy>("Goblin", new Vector3(-5, 0, 0)),
            UnityObject.Instantiate<FlyingEnemy>("Murciélago", new Vector3(5, 0, 0)),
            UnityObject.Instantiate<ArcherEnemy>("Esqueleto Arquero", new Vector3(-8, 0, 0)),
            UnityObject.Instantiate<FlyingArcherEnemy>("Wyvern", new Vector3(8, 0, 0)),
            UnityObject.Instantiate<BossEnemy>("Jefe Final", new Vector3(0, 0, -10)),
        };
```

- [ ] **Step 7: Run and verify sane output**

```bash
dotnet run --project src/Antes
```

Expected: 20 frames printed, all five enemy types approach and attack the player (melee
and ranged messages both appear), no exceptions thrown.

- [ ] **Step 8: Commit**

```bash
git add src/Antes
git commit -m "Antes: zoo de subclases de Enemy (explosión de herencia)"
```

---

### Task 4: `Despues.Domain` — datos y movimiento

**Files:**
- Create: `src/Despues/Despues.Domain/Despues.Domain.csproj` (via `dotnet new`)
- Create: `src/Despues/Despues.Domain/Vector3D.cs`
- Create: `src/Despues/Despues.Domain/EnemyStats.cs`
- Create: `src/Despues/Despues.Domain/IDamageable.cs`
- Create: `src/Despues/Despues.Domain/PlayerCharacter.cs`
- Create: `src/Despues/Despues.Domain/IMover.cs`
- Create: `src/Despues/Despues.Domain/GroundMover.cs`
- Create: `src/Despues/Despues.Domain/FlyingMover.cs`
- Create: `src/Despues/Despues.Domain/IAttacker.cs`
- Create: `src/Despues/Despues.Domain/MeleeAttack.cs`
- Delete: `src/Despues/Despues.Domain/Class1.cs` (template default)

**Interfaces:**
- Produces: `Despues.Domain.Vector3D` (fields `X,Y,Z`; `static float Distance(Vector3D,
  Vector3D)`; `static Vector3D MoveTowards(Vector3D current, Vector3D target, float
  maxDistanceDelta)`), `Despues.Domain.EnemyStats` (ctor `(string name, int maxHealth,
  float moveSpeed, int attackDamage, float attackRange, float attackCooldown)`; settable
  `Health`), `Despues.Domain.IDamageable` (`int Health { get; }`; `bool IsDead { get; }`;
  `void TakeDamage(int amount)`), `Despues.Domain.PlayerCharacter : IDamageable` (ctor
  `(string name, int maxHealth)`), `Despues.Domain.IMover` (`Vector3D Move(Vector3D
  currentPosition, Vector3D targetPosition, float speed, float deltaTime)`),
  `Despues.Domain.GroundMover : IMover`, `Despues.Domain.FlyingMover : IMover` (ctor
  `(float flightHeight)`), `Despues.Domain.IAttacker` (`string Name { get; }`; `bool
  TryAttack(EnemyStats attackerStats, Vector3D attackerPosition, IDamageable target,
  Vector3D targetPosition, float deltaTime, ref float cooldownTimer)`),
  `Despues.Domain.MeleeAttack : IAttacker` — consumed by Task 5 and Task 6.

No automated tests yet (these types don't have interesting branching on their own).
Verification is a successful build.

- [ ] **Step 1: Scaffold `Despues.Domain` and add it to the solution**

```bash
dotnet new classlib -n Despues.Domain -o src/Despues/Despues.Domain --framework net9.0
rm src/Despues/Despues.Domain/Class1.cs
dotnet sln add src/Despues/Despues.Domain/Despues.Domain.csproj
```

- [ ] **Step 2: Write `Vector3D.cs`**

```csharp
namespace Despues.Domain;

// Vector propio del dominio: distinto de UnityStubs.Vector3 a propósito.
// El dominio no conoce el vector del motor; el adaptador de Unity (Despues.Unity)
// es el que convierte entre uno y otro.
public struct Vector3D
{
    public float X;
    public float Y;
    public float Z;

    public Vector3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static float Distance(Vector3D a, Vector3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static Vector3D MoveTowards(Vector3D current, Vector3D target, float maxDistanceDelta)
    {
        var dx = target.X - current.X;
        var dy = target.Y - current.Y;
        var dz = target.Z - current.Z;
        var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance <= maxDistanceDelta || distance == 0f)
        {
            return target;
        }

        var t = maxDistanceDelta / distance;
        return new Vector3D(current.X + dx * t, current.Y + dy * t, current.Z + dz * t);
    }

    public override string ToString() => $"({X:0.0}, {Y:0.0}, {Z:0.0})";
}
```

- [ ] **Step 3: Write `EnemyStats.cs`**

```csharp
namespace Despues.Domain;

// POCO: solo datos. Nada de UnityEngine, nada de lógica de movimiento o ataque acá.
public class EnemyStats
{
    public string Name { get; }
    public int Health { get; set; }
    public int MaxHealth { get; }
    public float MoveSpeed { get; }
    public int AttackDamage { get; }
    public float AttackRange { get; }
    public float AttackCooldown { get; }

    public EnemyStats(string name, int maxHealth, float moveSpeed, int attackDamage, float attackRange, float attackCooldown)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
        MoveSpeed = moveSpeed;
        AttackDamage = attackDamage;
        AttackRange = attackRange;
        AttackCooldown = attackCooldown;
    }
}
```

- [ ] **Step 4: Write `IDamageable.cs` and `PlayerCharacter.cs`**

```csharp
namespace Despues.Domain;

public interface IDamageable
{
    int Health { get; }
    bool IsDead { get; }
    void TakeDamage(int amount);
}
```

```csharp
namespace Despues.Domain;

public class PlayerCharacter : IDamageable
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsDead => Health <= 0;

    public PlayerCharacter(string name, int maxHealth)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
    }
}
```

- [ ] **Step 5: Write `IMover.cs`, `GroundMover.cs` y `FlyingMover.cs`**

```csharp
namespace Despues.Domain;

public interface IMover
{
    Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime);
}
```

```csharp
namespace Despues.Domain;

public class GroundMover : IMover
{
    public Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime)
    {
        return Vector3D.MoveTowards(currentPosition, targetPosition, speed * deltaTime);
    }
}
```

```csharp
namespace Despues.Domain;

public class FlyingMover : IMover
{
    private readonly float _flightHeight;

    public FlyingMover(float flightHeight)
    {
        _flightHeight = flightHeight;
    }

    public Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime)
    {
        var hoverPoint = new Vector3D(targetPosition.X, targetPosition.Y + _flightHeight, targetPosition.Z);
        return Vector3D.MoveTowards(currentPosition, hoverPoint, speed * deltaTime);
    }
}
```

- [ ] **Step 6: Write `IAttacker.cs` y `MeleeAttack.cs`**

```csharp
namespace Despues.Domain;

public interface IAttacker
{
    string Name { get; }

    bool TryAttack(EnemyStats attackerStats, Vector3D attackerPosition, IDamageable target, Vector3D targetPosition, float deltaTime, ref float cooldownTimer);
}
```

```csharp
namespace Despues.Domain;

public class MeleeAttack : IAttacker
{
    public string Name => "Cuerpo a cuerpo";

    public bool TryAttack(EnemyStats attackerStats, Vector3D attackerPosition, IDamageable target, Vector3D targetPosition, float deltaTime, ref float cooldownTimer)
    {
        cooldownTimer -= deltaTime;

        var distance = Vector3D.Distance(attackerPosition, targetPosition);
        if (distance > attackerStats.AttackRange || cooldownTimer > 0f)
        {
            return false;
        }

        target.TakeDamage(attackerStats.AttackDamage);
        cooldownTimer = attackerStats.AttackCooldown;
        return true;
    }
}
```

- [ ] **Step 7: Build to verify it compiles**

```bash
dotnet build src/Despues/Despues.Domain/Despues.Domain.csproj
```

Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 8: Commit**

```bash
git add src/Despues/Despues.Domain
git commit -m "Despues.Domain: POCOs, IDamageable e IMover (sin dependencia de Unity)"
```

---

### Task 5: `EnemyController` + primeros tests xUnit

**Files:**
- Create: `tests/Despues.Tests/Despues.Tests.csproj` (via `dotnet new`)
- Create: `tests/Despues.Tests/EnemyControllerTests.cs`
- Create: `src/Despues/Despues.Domain/EnemyController.cs`
- Delete: `tests/Despues.Tests/UnitTest1.cs` (template default)

**Interfaces:**
- Consumes: `Despues.Domain.EnemyStats`, `IMover`, `IAttacker`, `IDamageable`,
  `GroundMover`, `MeleeAttack`, `Vector3D` (Task 4).
- Produces: `Despues.Domain.EnemyController : IDamageable` (ctor `(EnemyStats stats,
  Vector3D position, IMover mover, IAttacker attacker, IAttacker? enrageAttacker = null,
  float enrageHealthFraction = 0f)`; `EnemyStats Stats { get; }`; `Vector3D Position {
  get; }`; `IAttacker Attacker { get; }`; `void TakeDamage(int amount)`; `void
  Tick(Vector3D targetPosition, IDamageable target, float deltaTime)`) — consumed by
  Task 6, Task 7 y Task 8.

- [ ] **Step 1: Scaffold `Despues.Tests` and add it to the solution**

```bash
dotnet new xunit -n Despues.Tests -o tests/Despues.Tests --framework net9.0
rm tests/Despues.Tests/UnitTest1.cs
dotnet sln add tests/Despues.Tests/Despues.Tests.csproj
dotnet add tests/Despues.Tests/Despues.Tests.csproj reference src/Despues/Despues.Domain/Despues.Domain.csproj
```

- [ ] **Step 2: Write the failing tests in `EnemyControllerTests.cs`**

```csharp
using Despues.Domain;
using Xunit;

namespace Despues.Tests;

public class EnemyControllerTests
{
    private static EnemyController CreateController(int maxHealth)
    {
        var stats = new EnemyStats("Test", maxHealth, moveSpeed: 1f, attackDamage: 1, attackRange: 1f, attackCooldown: 1f);
        return new EnemyController(stats, new Vector3D(0, 0, 0), new GroundMover(), new MeleeAttack());
    }

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var controller = CreateController(maxHealth: 30);

        controller.TakeDamage(10);

        Assert.Equal(20, controller.Health);
    }

    [Fact]
    public void TakeDamage_WhenHealthReachesZero_MarksAsDead()
    {
        var controller = CreateController(maxHealth: 10);

        controller.TakeDamage(10);

        Assert.True(controller.IsDead);
    }

    [Fact]
    public void TakeDamage_NeverGoesBelowZero()
    {
        var controller = CreateController(maxHealth: 10);

        controller.TakeDamage(999);

        Assert.Equal(0, controller.Health);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail to build**

```bash
dotnet test tests/Despues.Tests/Despues.Tests.csproj
```

Expected: build error — `EnemyController` does not exist yet.

- [ ] **Step 4: Implement `EnemyController.cs`**

```csharp
namespace Despues.Domain;

// La composición en acción: un EnemyController no hereda de nada especial de
// gameplay, simplemente compone stats + una forma de moverse + una forma de
// atacar. Combinar "vuela" con "ataca a distancia" es pasarle un FlyingMover
// y un RangedAttack — no hace falta ninguna clase nueva (ver EnemyFactory).
public class EnemyController : IDamageable
{
    public EnemyStats Stats { get; }
    public Vector3D Position { get; private set; }
    public IAttacker Attacker { get; private set; }

    private readonly IMover _mover;
    private readonly IAttacker? _enrageAttacker;
    private readonly float _enrageHealthFraction;
    private float _cooldownTimer;

    public int Health => Stats.Health;
    public bool IsDead => Stats.Health <= 0;

    public EnemyController(EnemyStats stats, Vector3D position, IMover mover, IAttacker attacker, IAttacker? enrageAttacker = null, float enrageHealthFraction = 0f)
    {
        Stats = stats;
        Position = position;
        _mover = mover;
        Attacker = attacker;
        _enrageAttacker = enrageAttacker;
        _enrageHealthFraction = enrageHealthFraction;
    }

    public void TakeDamage(int amount)
    {
        Stats.Health = Math.Max(0, Stats.Health - amount);
    }

    public void Tick(Vector3D targetPosition, IDamageable target, float deltaTime)
    {
        if (IsDead)
        {
            return;
        }

        // Cambio de comportamiento en tiempo de ejecución vía polimorfismo:
        // reasignamos el IAttacker sin condicionales por tipo ni herencia nueva.
        if (_enrageAttacker != null && Attacker != _enrageAttacker && Stats.Health <= Stats.MaxHealth * _enrageHealthFraction)
        {
            Attacker = _enrageAttacker;
        }

        var distance = Vector3D.Distance(Position, targetPosition);
        if (distance > Stats.AttackRange)
        {
            Position = _mover.Move(Position, targetPosition, Stats.MoveSpeed, deltaTime);
            return;
        }

        Attacker.TryAttack(Stats, Position, target, targetPosition, deltaTime, ref _cooldownTimer);
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
dotnet test tests/Despues.Tests/Despues.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 3`.

- [ ] **Step 6: Commit**

```bash
git add tests/Despues.Tests src/Despues/Despues.Domain/EnemyController.cs
git commit -m "EnemyController: composición testeable, sin depender de Unity"
```

---

### Task 6: `RangedAttack` + `EnemyFactory`

**Files:**
- Create: `src/Despues/Despues.Domain/IProjectileSpawner.cs`
- Create: `src/Despues/Despues.Domain/RangedAttack.cs`
- Create: `tests/Despues.Tests/RangedAttackTests.cs`
- Create: `src/Despues/Despues.Domain/EnemyFactory.cs`

**Interfaces:**
- Consumes: `Despues.Domain.IAttacker`, `EnemyStats`, `Vector3D`, `IDamageable` (Task 4);
  `EnemyController`, `GroundMover`, `FlyingMover`, `MeleeAttack` (Task 4/5).
- Produces: `Despues.Domain.IProjectileSpawner` (`void Spawn(Vector3D from, Vector3D to,
  int damage, IDamageable target)`), `Despues.Domain.RangedAttack : IAttacker` (ctor
  `(IProjectileSpawner spawner)`), `Despues.Domain.EnemyFactory` (static
  `CreateGoblin(Vector3D position)`, `CreateSkeletonArcher(Vector3D position,
  IProjectileSpawner spawner)`, `CreateBat(Vector3D position)`,
  `CreateFlyingArcher(Vector3D position, IProjectileSpawner spawner)`,
  `CreateBoss(Vector3D position, IProjectileSpawner spawner)`, all returning
  `EnemyController`) — consumed by Task 7 y Task 8.

- [ ] **Step 1: Write `IProjectileSpawner.cs`**

```csharp
namespace Despues.Domain;

// Puerto: el dominio pide "lanzar un proyectil" sin saber cómo se representa
// visualmente. Cada entorno de ejecución trae su propia implementación
// (Despues.Unity y Despues.DomainConsole).
public interface IProjectileSpawner
{
    void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target);
}
```

- [ ] **Step 2: Write the failing tests in `RangedAttackTests.cs`**

```csharp
using Despues.Domain;
using Xunit;

namespace Despues.Tests;

public class RangedAttackTests
{
    private class SpySpawner : IProjectileSpawner
    {
        public int SpawnCount { get; private set; }

        public void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target)
        {
            SpawnCount++;
        }
    }

    private class DummyTarget : IDamageable
    {
        public int Health => 100;
        public bool IsDead => false;
        public void TakeDamage(int amount) { }
    }

    private static EnemyStats CreateStats() =>
        new("Test", maxHealth: 30, moveSpeed: 1f, attackDamage: 5, attackRange: 8f, attackCooldown: 2f);

    [Fact]
    public void TryAttack_WhenInRangeAndCooldownElapsed_SpawnsProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 0f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(5, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.True(attacked);
        Assert.Equal(1, spawner.SpawnCount);
    }

    [Fact]
    public void TryAttack_WhenOutOfRange_DoesNotSpawnProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 0f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(50, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.False(attacked);
        Assert.Equal(0, spawner.SpawnCount);
    }

    [Fact]
    public void TryAttack_WhenCooldownNotElapsed_DoesNotSpawnProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 5f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(5, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.False(attacked);
        Assert.Equal(0, spawner.SpawnCount);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail to build**

```bash
dotnet test tests/Despues.Tests/Despues.Tests.csproj
```

Expected: build error — `RangedAttack` does not exist yet.

- [ ] **Step 4: Implement `RangedAttack.cs`**

```csharp
namespace Despues.Domain;

public class RangedAttack : IAttacker
{
    private readonly IProjectileSpawner _spawner;

    public RangedAttack(IProjectileSpawner spawner)
    {
        _spawner = spawner;
    }

    public string Name => "A distancia";

    public bool TryAttack(EnemyStats attackerStats, Vector3D attackerPosition, IDamageable target, Vector3D targetPosition, float deltaTime, ref float cooldownTimer)
    {
        cooldownTimer -= deltaTime;

        var distance = Vector3D.Distance(attackerPosition, targetPosition);
        if (distance > attackerStats.AttackRange || cooldownTimer > 0f)
        {
            return false;
        }

        _spawner.Spawn(attackerPosition, targetPosition, attackerStats.AttackDamage, target);
        cooldownTimer = attackerStats.AttackCooldown;
        return true;
    }
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
dotnet test tests/Despues.Tests/Despues.Tests.csproj
```

Expected: `Passed! - Failed: 0, Passed: 6`.

- [ ] **Step 6: Implement `EnemyFactory.cs`**

```csharp
namespace Despues.Domain;

public static class EnemyFactory
{
    public static EnemyController CreateGoblin(Vector3D position)
    {
        var stats = new EnemyStats("Goblin", maxHealth: 30, moveSpeed: 2f, attackDamage: 5, attackRange: 1.5f, attackCooldown: 1.5f);
        return new EnemyController(stats, position, new GroundMover(), new MeleeAttack());
    }

    public static EnemyController CreateSkeletonArcher(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Esqueleto Arquero", maxHealth: 25, moveSpeed: 1.5f, attackDamage: 3, attackRange: 8f, attackCooldown: 2f);
        return new EnemyController(stats, position, new GroundMover(), new RangedAttack(spawner));
    }

    public static EnemyController CreateBat(Vector3D position)
    {
        var stats = new EnemyStats("Murciélago", maxHealth: 20, moveSpeed: 3f, attackDamage: 4, attackRange: 1.5f, attackCooldown: 1f);
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 3f), new MeleeAttack());
    }

    // La combinación "vuela + ataca a distancia" que en Antes forzaba copiar y
    // pegar código (FlyingArcherEnemy) acá es una sola llamada, sin clase nueva.
    public static EnemyController CreateFlyingArcher(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Wyvern", maxHealth: 35, moveSpeed: 2.5f, attackDamage: 3, attackRange: 8f, attackCooldown: 2f);
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 4f), new RangedAttack(spawner));
    }

    public static EnemyController CreateBoss(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Jefe Final", maxHealth: 120, moveSpeed: 2f, attackDamage: 4, attackRange: 8f, attackCooldown: 2f);
        var rangedAttack = new RangedAttack(spawner);
        var enragedMelee = new MeleeAttack();
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 2f), rangedAttack, enrageAttacker: enragedMelee, enrageHealthFraction: 0.3f);
    }
}
```

- [ ] **Step 7: Build to verify everything still compiles**

```bash
dotnet build src/Despues/Despues.Domain/Despues.Domain.csproj
```

Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 8: Commit**

```bash
git add src/Despues/Despues.Domain/IProjectileSpawner.cs src/Despues/Despues.Domain/RangedAttack.cs src/Despues/Despues.Domain/EnemyFactory.cs tests/Despues.Tests/RangedAttackTests.cs
git commit -m "RangedAttack + EnemyFactory: la combinación imposible en Antes ahora es una línea"
```

---

### Task 7: `Despues.Unity` — el adaptador delgado

**Files:**
- Create: `src/Despues/Despues.Unity/Despues.Unity.csproj` (via `dotnet new`)
- Create: `src/Despues/Despues.Unity/UnityProjectileSpawner.cs`
- Create: `src/Despues/Despues.Unity/EnemyView.cs`
- Create: `src/Despues/Despues.Unity/Program.cs`

**Interfaces:**
- Consumes: `Despues.Domain.EnemyController`, `EnemyFactory`, `IProjectileSpawner`,
  `PlayerCharacter`, `Vector3D` (Task 4/5/6); `UnityStubs.MonoBehaviour`, `GameObject`,
  `Transform`, `Vector3`, `Time`, `Debug`, `UnityObject` (Task 1).

No automated tests for this project — it is the adapter layer; verification is running it
and comparing the output to `Antes` and to `Despues.DomainConsole` (Task 8).

- [ ] **Step 1: Scaffold `Despues.Unity` and add it to the solution**

```bash
dotnet new console -n Despues.Unity -o src/Despues/Despues.Unity --framework net9.0
dotnet sln add src/Despues/Despues.Unity/Despues.Unity.csproj
dotnet add src/Despues/Despues.Unity/Despues.Unity.csproj reference src/Despues/Despues.Domain/Despues.Domain.csproj
dotnet add src/Despues/Despues.Unity/Despues.Unity.csproj reference src/UnityStubs/UnityStubs.csproj
```

- [ ] **Step 2: Write `UnityProjectileSpawner.cs`**

```csharp
using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

public class UnityProjectileSpawner : IProjectileSpawner
{
    public void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target)
    {
        Debug.Log($"[Proyectil] vuela desde {from} y golpea por {damage}");
        target.TakeDamage(damage);
    }
}
```

- [ ] **Step 3: Write `EnemyView.cs`**

```csharp
using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

// Adaptador delgado: convierte entre los tipos de UnityStubs y los del dominio,
// y delega toda la lógica de gameplay en EnemyController. No hay reglas de
// combate acá adentro.
public class EnemyView : MonoBehaviour
{
    private EnemyController _controller = null!;
    private Transform _targetTransform = null!;
    private IDamageable _target = null!;

    public void Initialize(EnemyController controller, Transform targetTransform, IDamageable target)
    {
        _controller = controller;
        _targetTransform = targetTransform;
        _target = target;
        transform.position = ToUnity(controller.Position);
    }

    public override void Update()
    {
        if (_controller.IsDead)
        {
            return;
        }

        var attackerBefore = _controller.Attacker.Name;
        var targetPosition = ToDomain(_targetTransform.position);
        _controller.Tick(targetPosition, _target, Time.deltaTime);
        transform.position = ToUnity(_controller.Position);

        if (_controller.Attacker.Name != attackerBefore)
        {
            Debug.Log($"[{_controller.Stats.Name}] ¡cambia su forma de atacar a \"{_controller.Attacker.Name}\"!");
        }
    }

    private static Vector3 ToUnity(Vector3D v) => new(v.X, v.Y, v.Z);
    private static Vector3D ToDomain(Vector3 v) => new(v.x, v.y, v.z);
}
```

- [ ] **Step 4: Write `Program.cs`**

```csharp
using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

public static class Program
{
    public static void Main()
    {
        var playerGameObject = new GameObject("Jugador", new Vector3(0, 0, 0));
        var player = new PlayerCharacter("Jugador", maxHealth: 100);
        var spawner = new UnityProjectileSpawner();

        var enemyPlacements = new (EnemyController Controller, Vector3 Position)[]
        {
            (EnemyFactory.CreateGoblin(new Vector3D(-5, 0, 0)), new Vector3(-5, 0, 0)),
            (EnemyFactory.CreateBat(new Vector3D(5, 0, 0)), new Vector3(5, 0, 0)),
            (EnemyFactory.CreateSkeletonArcher(new Vector3D(-8, 0, 0), spawner), new Vector3(-8, 0, 0)),
            (EnemyFactory.CreateFlyingArcher(new Vector3D(8, 0, 0), spawner), new Vector3(8, 0, 0)),
            (EnemyFactory.CreateBoss(new Vector3D(0, 0, -10), spawner), new Vector3(0, 0, -10)),
        };

        var enemyViews = new List<EnemyView>();
        foreach (var (controller, position) in enemyPlacements)
        {
            var view = UnityObject.Instantiate<EnemyView>(controller.Stats.Name, position);
            view.Initialize(controller, playerGameObject.transform, player);
            enemyViews.Add(view);
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Time.deltaTime = deltaTime;
            Debug.Log($"--- Frame {frame} ---");

            foreach (var view in enemyViews)
            {
                view.Update();
            }

            if (player.IsDead)
            {
                Debug.Log("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
```

- [ ] **Step 5: Run and verify sane output**

```bash
dotnet run --project src/Despues/Despues.Unity
```

Expected: 20 frames printed, all five enemy types act, and the boss prints a
`cambia su forma de atacar a "Cuerpo a cuerpo"` line once its health drops below 30%.

- [ ] **Step 6: Commit**

```bash
git add src/Despues/Despues.Unity
git commit -m "Despues.Unity: EnemyView como adaptador delgado sobre EnemyController"
```

---

### Task 8: `Despues.DomainConsole` — la misma partida sin Unity

**Files:**
- Create: `src/Despues/Despues.DomainConsole/Despues.DomainConsole.csproj` (via `dotnet new`)
- Create: `src/Despues/Despues.DomainConsole/ConsoleProjectileSpawner.cs`
- Create: `src/Despues/Despues.DomainConsole/Program.cs`

**Interfaces:**
- Consumes: `Despues.Domain.EnemyController`, `EnemyFactory`, `IProjectileSpawner`,
  `PlayerCharacter`, `Vector3D` (Task 4/5/6). No reference to `UnityStubs` at all.

No automated tests — same reasoning as Task 7. Verification is running it and comparing
against `Despues.Unity`.

- [ ] **Step 1: Scaffold `Despues.DomainConsole` and add it to the solution**

```bash
dotnet new console -n Despues.DomainConsole -o src/Despues/Despues.DomainConsole --framework net9.0
dotnet sln add src/Despues/Despues.DomainConsole/Despues.DomainConsole.csproj
dotnet add src/Despues/Despues.DomainConsole/Despues.DomainConsole.csproj reference src/Despues/Despues.Domain/Despues.Domain.csproj
```

- [ ] **Step 2: Write `ConsoleProjectileSpawner.cs`**

```csharp
using Despues.Domain;

namespace Despues.DomainConsole;

public class ConsoleProjectileSpawner : IProjectileSpawner
{
    public void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target)
    {
        Console.WriteLine($"[Proyectil] vuela desde {from} y golpea por {damage}");
        target.TakeDamage(damage);
    }
}
```

- [ ] **Step 3: Write `Program.cs`**

```csharp
using Despues.Domain;

namespace Despues.DomainConsole;

public static class Program
{
    public static void Main()
    {
        var player = new PlayerCharacter("Jugador", maxHealth: 100);
        var playerPosition = new Vector3D(0, 0, 0);
        var spawner = new ConsoleProjectileSpawner();

        var enemies = new List<EnemyController>
        {
            EnemyFactory.CreateGoblin(new Vector3D(-5, 0, 0)),
            EnemyFactory.CreateBat(new Vector3D(5, 0, 0)),
            EnemyFactory.CreateSkeletonArcher(new Vector3D(-8, 0, 0), spawner),
            EnemyFactory.CreateFlyingArcher(new Vector3D(8, 0, 0), spawner),
            EnemyFactory.CreateBoss(new Vector3D(0, 0, -10), spawner),
        };

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Console.WriteLine($"--- Frame {frame} ---");

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead)
                {
                    continue;
                }

                var attackerBefore = enemy.Attacker.Name;
                enemy.Tick(playerPosition, player, deltaTime);

                if (enemy.Attacker.Name != attackerBefore)
                {
                    Console.WriteLine($"[{enemy.Stats.Name}] ¡cambia su forma de atacar a \"{enemy.Attacker.Name}\"!");
                }
            }

            if (player.IsDead)
            {
                Console.WriteLine("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
```

- [ ] **Step 4: Run and verify sane output**

```bash
dotnet run --project src/Despues/Despues.DomainConsole
```

Expected: the same kind of frame-by-frame log as `Despues.Unity` (Task 7), including the
boss's attacker-change line, with zero reference to any `UnityStubs` type anywhere in this
project's source.

- [ ] **Step 5: Confirm `Despues.Domain` has no Unity dependency**

```bash
grep -R "UnityStubs" src/Despues/Despues.Domain || echo "OK: sin referencias a UnityStubs"
```

Expected: `OK: sin referencias a UnityStubs`.

- [ ] **Step 6: Commit**

```bash
git add src/Despues/Despues.DomainConsole
git commit -m "Despues.DomainConsole: la misma partida corriendo sin Unity"
```

---

### Task 9: README de la clase y verificación final de la solución

**Files:**
- Create: `README.md`

**Interfaces:** none (documentation only).

- [ ] **Step 1: Write `README.md`**

```markdown
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
```

- [ ] **Step 2: Full-solution build and test**

```bash
dotnet build
dotnet test
```

Expected: `Build succeeded` for the whole solution, and `Passed! - Failed: 0, Passed: 6`
for the test run.

- [ ] **Step 3: Run all three demos one more time as a final sanity check**

```bash
dotnet run --project src/Antes
dotnet run --project src/Despues/Despues.Unity
dotnet run --project src/Despues/Despues.DomainConsole
```

Expected: all three run to completion without exceptions.

- [ ] **Step 4: Commit**

```bash
git add README.md
git commit -m "Agregar README de la clase 05 con guía de refactor en vivo"
```
