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
