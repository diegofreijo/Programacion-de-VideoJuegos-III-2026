using UnityStubs;

namespace Antes;

public class FlyingEnemy : Enemy
{
    public float flightHeight = 3f;

    // Tenemos que reescribir Update() entero solo para cambiar el punto
    // hacia el que se mueve (vuela por encima del jugador). El resto
    // (ataque, cooldown, muerte) queda duplicado tal cual estaba en Enemy.
    public override void Update()
    {
        if (health <= 0 || gameObject.isDestroyed)
        {
            return;
        }

        var hoverPoint = new Vector3(target.transform.position.x, target.transform.position.y + flightHeight, target.transform.position.z);
        var distance = Vector3.Distance(transform.position, hoverPoint);

        if (distance > attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverPoint, moveSpeed * Time.deltaTime);
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Attack(target);
            cooldownTimer = attackCooldown;
        }
    }
}
