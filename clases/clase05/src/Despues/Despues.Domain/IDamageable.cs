namespace Despues.Domain;

public interface IDamageable
{
    int Health { get; }
    bool IsDead { get; }
    void TakeDamage(int amount);
}
