using UnityStubs;

namespace Antes;

public class Player : MonoBehaviour
{
    public int health = 100;
    public int maxHealth = 100;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0)
        {
            health = 0;
        }

        Debug.Log($"[Jugador] recibe {amount} de daño. Vida: {health}/{maxHealth}");
    }
}
