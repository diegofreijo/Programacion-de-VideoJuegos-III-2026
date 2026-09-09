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
