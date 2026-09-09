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
