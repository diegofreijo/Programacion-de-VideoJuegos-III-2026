using Despues.Domain;

namespace Despues.DomainConsole;

public static class Program
{
    public static void Main()
    {
        var player = new PlayerCharacter("Jugador", maxHealth: 100);
        var playerPosition = new Vector3D(0, 0, 0);
        var spawner = new ConsoleProjectileSpawner();

        var enemies = new List<EnemyController>
        {
            EnemyFactory.CreateGoblin(new Vector3D(-5, 0, 0)),
            EnemyFactory.CreateBat(new Vector3D(5, 0, 0)),
            EnemyFactory.CreateSkeletonArcher(new Vector3D(-8, 0, 0), spawner),
            EnemyFactory.CreateFlyingArcher(new Vector3D(8, 0, 0), spawner),
            EnemyFactory.CreateBoss(new Vector3D(0, 0, -10), spawner),
        };

        // Guarda una referencia al jefe para poder disparar un evento de daño scripted más adelante
        EnemyController? bossController = null;
        foreach (var enemy in enemies)
        {
            if (enemy.Stats.Name == "Jefe Final")
            {
                bossController = enemy;
                break;
            }
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Console.WriteLine($"--- Frame {frame} ---");

            // Evento scripted en frame 10: el jugador logra herir gravemente al jefe
            // Esto desencadena el cambio de atacante (de RangedAttack a MeleeAttack).
            if (frame == 10 && bossController != null)
            {
                Console.WriteLine("[Simulación] El jugador logra herir gravemente al jefe.");
                bossController.TakeDamage(90);
            }

            foreach (var enemy in enemies)
            {
                if (enemy.IsDead)
                {
                    continue;
                }

                var attackerBefore = enemy.Attacker.Name;
                enemy.Tick(playerPosition, player, deltaTime);

                if (enemy.Attacker.Name != attackerBefore)
                {
                    Console.WriteLine($"[{enemy.Stats.Name}] ¡cambia su forma de atacar a \"{enemy.Attacker.Name}\"!");
                }
            }

            if (player.IsDead)
            {
                Console.WriteLine("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
