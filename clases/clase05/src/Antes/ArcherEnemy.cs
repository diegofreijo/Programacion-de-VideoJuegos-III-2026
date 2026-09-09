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
