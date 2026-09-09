using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

public class UnityProjectileSpawner : IProjectileSpawner
{
    public void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target)
    {
        var projectile = UnityObject.Instantiate<ProjectileView>("Proyectil", new Vector3(from.X, from.Y, from.Z));
        Debug.Log($"[Proyectil] vuela desde {from} y golpea por {damage}");
        target.TakeDamage(damage);
        UnityObject.Destroy(projectile.gameObject);
    }
}
