using Despues.Domain;
using Xunit;

namespace Despues.Tests;

public class EnemyControllerTests
{
    private static EnemyController CreateController(int maxHealth)
    {
        var stats = new EnemyStats("Test", maxHealth, moveSpeed: 1f, attackDamage: 1, attackRange: 1f, attackCooldown: 1f);
        return new EnemyController(stats, new Vector3D(0, 0, 0), new GroundMover(), new MeleeAttack());
    }

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var controller = CreateController(maxHealth: 30);

        controller.TakeDamage(10);

        Assert.Equal(20, controller.Health);
    }

    [Fact]
    public void TakeDamage_WhenHealthReachesZero_MarksAsDead()
    {
        var controller = CreateController(maxHealth: 10);

        controller.TakeDamage(10);

        Assert.True(controller.IsDead);
    }

    [Fact]
    public void TakeDamage_NeverGoesBelowZero()
    {
        var controller = CreateController(maxHealth: 10);

        controller.TakeDamage(999);

        Assert.Equal(0, controller.Health);
    }
}
