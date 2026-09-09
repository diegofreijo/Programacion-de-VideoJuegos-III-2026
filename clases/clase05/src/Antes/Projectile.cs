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
