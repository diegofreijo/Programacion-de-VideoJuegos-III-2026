using UnityStubs;

namespace Antes;

public static class Program
{
    public static void Main()
    {
        var player = UnityObject.Instantiate<Player>("Jugador", new Vector3(0, 0, 0));

        var boss = UnityObject.Instantiate<BossEnemy>("Jefe Final", new Vector3(0, 0, -10));

        var enemies = new List<Enemy>
        {
            UnityObject.Instantiate<Enemy>("Goblin", new Vector3(-5, 0, 0)),
            UnityObject.Instantiate<FlyingEnemy>("Murciélago", new Vector3(5, 0, 0)),
            UnityObject.Instantiate<ArcherEnemy>("Esqueleto Arquero", new Vector3(-8, 0, 0)),
            UnityObject.Instantiate<FlyingArcherEnemy>("Wyvern", new Vector3(8, 0, 0)),
            boss,
        };

        foreach (var enemy in enemies)
        {
            enemy.enemyName = enemy.gameObject.name;
            enemy.SetTarget(player);
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Time.deltaTime = deltaTime;
            Debug.Log($"--- Frame {frame} ---");

            // Evento scripted en frame 10: el jugador logra herir gravemente al jefe.
            // Deja su vida por debajo del umbral de enfurecimiento (30% de 120 = 36).
            if (frame == 10)
            {
                Debug.Log("[Simulación] El jugador logra herir gravemente al jefe.");
                boss.TakeDamage(90);
            }

            foreach (var enemy in enemies)
            {
                enemy.Update();
            }

            if (player.health <= 0)
            {
                Debug.Log("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
