using Despues.Domain;
using Xunit;

namespace Despues.Tests;

public class RangedAttackTests
{
    private class SpySpawner : IProjectileSpawner
    {
        public int SpawnCount { get; private set; }

        public void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target)
        {
            SpawnCount++;
        }
    }

    private class DummyTarget : IDamageable
    {
        public int Health => 100;
        public bool IsDead => false;
        public void TakeDamage(int amount) { }
    }

    private static EnemyStats CreateStats() =>
        new("Test", maxHealth: 30, moveSpeed: 1f, attackDamage: 5, attackRange: 8f, attackCooldown: 2f);

    [Fact]
    public void TryAttack_WhenInRangeAndCooldownElapsed_SpawnsProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 0f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(5, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.True(attacked);
        Assert.Equal(1, spawner.SpawnCount);
    }

    [Fact]
    public void TryAttack_WhenOutOfRange_DoesNotSpawnProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 0f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(50, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.False(attacked);
        Assert.Equal(0, spawner.SpawnCount);
    }

    [Fact]
    public void TryAttack_WhenCooldownNotElapsed_DoesNotSpawnProjectile()
    {
        var spawner = new SpySpawner();
        var attack = new RangedAttack(spawner);
        var cooldownTimer = 5f;

        var attacked = attack.TryAttack(CreateStats(), new Vector3D(0, 0, 0), new DummyTarget(), new Vector3D(5, 0, 0), deltaTime: 0.1f, ref cooldownTimer);

        Assert.False(attacked);
        Assert.Equal(0, spawner.SpawnCount);
    }
}
