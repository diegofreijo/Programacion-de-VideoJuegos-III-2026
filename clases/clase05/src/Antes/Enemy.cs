using UnityStubs;

namespace Antes;

// OJO: esta clase mezcla varias razones para cambiar en un solo lugar:
// - el programador de gameplay (fórmula de daño, rango, cooldown)
// - el de sonido/feedback (qué se loguea al atacar o morir)
// - el de UI (la "barra de vida")
// - el diseñador (balance de stats)
// Cualquiera de ellos termina tocando este mismo archivo.
public class Enemy : MonoBehaviour
{
    public string enemyName = "Enemigo";
    public int health = 30;
    public int maxHealth = 30;
    public float moveSpeed = 2f;
    public int attackDamage = 5;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    protected float cooldownTimer;
    protected Player target = null!;

    public void SetTarget(Player player) => target = player;

    public override void Update()
    {
        if (health <= 0 || gameObject.isDestroyed)
        {
            return;
        }

        var distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, moveSpeed * Time.deltaTime);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Attack(target);
            cooldownTimer = attackCooldown;
        }
    }

    protected virtual void Attack(Player targetPlayer)
    {
        Debug.Log($"[{enemyName}] ataca cuerpo a cuerpo por {attackDamage}");
        targetPlayer.TakeDamage(attackDamage);
    }

    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[{enemyName}] recibe {amount} de daño. Vida: {health}/{maxHealth}");
        UpdateHealthBarUI();

        if (health <= 0)
        {
            Die();
        }
    }

    protected void UpdateHealthBarUI()
    {
        // simula actualizar una barra de vida en pantalla
        Debug.Log($"[UI] Barra de vida de {enemyName}: {health}/{maxHealth}");
    }

    protected virtual void Die()
    {
        Debug.Log($"[{enemyName}] murió");
        UnityObject.Destroy(gameObject);
    }
}
