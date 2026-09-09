namespace Despues.Domain;

public static class EnemyFactory
{
    public static EnemyController CreateGoblin(Vector3D position)
    {
        var stats = new EnemyStats("Goblin", maxHealth: 30, moveSpeed: 2f, attackDamage: 5, attackRange: 1.5f, attackCooldown: 1.5f);
        return new EnemyController(stats, position, new GroundMover(), new MeleeAttack());
    }

    public static EnemyController CreateSkeletonArcher(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Esqueleto Arquero", maxHealth: 25, moveSpeed: 1.5f, attackDamage: 3, attackRange: 8f, attackCooldown: 2f);
        return new EnemyController(stats, position, new GroundMover(), new RangedAttack(spawner));
    }

    public static EnemyController CreateBat(Vector3D position)
    {
        var stats = new EnemyStats("Murciélago", maxHealth: 20, moveSpeed: 3f, attackDamage: 4, attackRange: 1.5f, attackCooldown: 1f);
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 3f), new MeleeAttack());
    }

    // La combinación "vuela + ataca a distancia" que en Antes forzaba copiar y
    // pegar código (FlyingArcherEnemy) acá es una sola llamada, sin clase nueva.
    public static EnemyController CreateFlyingArcher(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Wyvern", maxHealth: 35, moveSpeed: 2.5f, attackDamage: 3, attackRange: 8f, attackCooldown: 2f);
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 4f), new RangedAttack(spawner));
    }

    public static EnemyController CreateBoss(Vector3D position, IProjectileSpawner spawner)
    {
        var stats = new EnemyStats("Jefe Final", maxHealth: 120, moveSpeed: 2f, attackDamage: 4, attackRange: 8f, attackCooldown: 2f);
        var rangedAttack = new RangedAttack(spawner);
        var enragedMelee = new MeleeAttack();
        return new EnemyController(stats, position, new FlyingMover(flightHeight: 2f), rangedAttack, enrageAttacker: enragedMelee, enrageHealthFraction: 0.3f);
    }
}
