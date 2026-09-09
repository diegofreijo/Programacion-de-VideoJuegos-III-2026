using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

public static class Program
{
    public static void Main()
    {
        var playerGameObject = new GameObject("Jugador", new Vector3(0, 0, 0));
        var player = new PlayerCharacter("Jugador", maxHealth: 100);
        var spawner = new UnityProjectileSpawner();

        var enemyPlacements = new (EnemyController Controller, Vector3 Position)[]
        {
            (EnemyFactory.CreateGoblin(new Vector3D(-5, 0, 0)), new Vector3(-5, 0, 0)),
            (EnemyFactory.CreateBat(new Vector3D(5, 0, 0)), new Vector3(5, 0, 0)),
            (EnemyFactory.CreateSkeletonArcher(new Vector3D(-8, 0, 0), spawner), new Vector3(-8, 0, 0)),
            (EnemyFactory.CreateFlyingArcher(new Vector3D(8, 0, 0), spawner), new Vector3(8, 0, 0)),
            (EnemyFactory.CreateBoss(new Vector3D(0, 0, -10), spawner), new Vector3(0, 0, -10)),
        };

        var enemyViews = new List<EnemyView>();
        EnemyController? bossController = null;
        foreach (var (controller, position) in enemyPlacements)
        {
            var view = UnityObject.Instantiate<EnemyView>(controller.Stats.Name, position);
            view.Initialize(controller, playerGameObject.transform, player);
            enemyViews.Add(view);

            // Guarda una referencia al jefe para poder disparar un evento de daño scripted más adelante
            if (controller.Stats.Name == "Jefe Final")
            {
                bossController = controller;
            }
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Time.deltaTime = deltaTime;
            Debug.Log($"--- Frame {frame} ---");

            // Evento scripted en frame 10: el jugador logra herir gravemente al jefe
            // Esto desencadena el cambio de atacante (de RangedAttack a MeleeAttack).
            if (frame == 10 && bossController != null)
            {
                Debug.Log("[Simulación] El jugador logra herir gravemente al jefe.");
                bossController.TakeDamage(90);
            }

            foreach (var view in enemyViews)
            {
                view.Update();
            }

            if (player.IsDead)
            {
                Debug.Log("El jugador murió. Fin de la simulación.");
                break;
            }
        }
    }
}
