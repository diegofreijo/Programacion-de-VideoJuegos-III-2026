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
