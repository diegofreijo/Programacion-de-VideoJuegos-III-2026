using Despues.Domain;
using UnityStubs;

namespace Despues.Unity;

// Adaptador delgado: convierte entre los tipos de UnityStubs y los del dominio,
// y delega toda la lógica de gameplay en EnemyController. No hay reglas de
// combate acá adentro.
public class EnemyView : MonoBehaviour
{
    private EnemyController _controller = null!;
    private Transform _targetTransform = null!;
    private IDamageable _target = null!;

    public void Initialize(EnemyController controller, Transform targetTransform, IDamageable target)
    {
        _controller = controller;
        _targetTransform = targetTransform;
        _target = target;
        transform.position = ToUnity(controller.Position);
    }

    public override void Update()
    {
        if (_controller.IsDead)
        {
            return;
        }

        var attackerBefore = _controller.Attacker.Name;
        var targetPosition = ToDomain(_targetTransform.position);
        _controller.Tick(targetPosition, _target, Time.deltaTime);
        transform.position = ToUnity(_controller.Position);

        if (_controller.Attacker.Name != attackerBefore)
        {
            Debug.Log($"[{_controller.Stats.Name}] ¡cambia su forma de atacar a \"{_controller.Attacker.Name}\"!");
        }
    }

    private static Vector3 ToUnity(Vector3D v) => new(v.X, v.Y, v.Z);
    private static Vector3D ToDomain(Vector3 v) => new(v.x, v.y, v.z);
}
