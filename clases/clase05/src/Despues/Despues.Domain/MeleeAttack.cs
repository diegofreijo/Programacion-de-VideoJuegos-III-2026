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
