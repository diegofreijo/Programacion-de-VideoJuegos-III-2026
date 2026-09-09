namespace Despues.Domain;

public class PlayerCharacter : IDamageable
{
    public string Name { get; }
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public bool IsDead => Health <= 0;

    public PlayerCharacter(string name, int maxHealth)
    {
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
    }
}
