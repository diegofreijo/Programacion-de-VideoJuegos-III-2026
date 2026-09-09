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
