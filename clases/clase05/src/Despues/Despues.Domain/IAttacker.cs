namespace Despues.Domain;

public interface IAttacker
{
    string Name { get; }

    bool TryAttack(EnemyStats attackerStats, Vector3D attackerPosition, IDamageable target, Vector3D targetPosition, float deltaTime, ref float cooldownTimer);
}
