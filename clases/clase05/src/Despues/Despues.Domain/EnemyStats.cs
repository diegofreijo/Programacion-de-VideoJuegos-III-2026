namespace Despues.Domain;

// POCO: solo datos. Nada de UnityEngine, nada de lógica de movimiento o ataque acá.
public class EnemyStats
{
    public string Name { get; }
    public int Health { get; set; }
    public int MaxHealth { get; }
    public float MoveSpeed { get; }
    public int AttackDamage { get; }
    public float AttackRange { get; }
    public float AttackCooldown { get; }

    public EnemyStats(string name, int maxHealth, float moveSpeed, int attackDamage, float attackRange, float attackCooldown)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
        MoveSpeed = moveSpeed;
        AttackDamage = attackDamage;
        AttackRange = attackRange;
        AttackCooldown = attackCooldown;
    }
}
