using UnityStubs;

namespace Antes;

public static class Program
{
    public static void Main()
    {
        var player = UnityObject.Instantiate<Player>("Jugador", new Vector3(0, 0, 0));

        var enemies = new List<Enemy>
        {
            UnityObject.Instantiate<Enemy>("Goblin", new Vector3(-5, 0, 0)),
        };

        foreach (var enemy in enemies)
        {
            enemy.SetTarget(player);
        }

        const int frameCount = 20;
        const float deltaTime = 0.5f;

        for (var frame = 1; frame <= frameCount; frame++)
        {
            Time.deltaTime = deltaTime;
            Debug.Log($"--- Frame {frame} ---");

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
