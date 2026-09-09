namespace Despues.Domain;

// La composición en acción: un EnemyController no hereda de nada especial de
// gameplay, simplemente compone stats + una forma de moverse + una forma de
// atacar. Combinar "vuela" con "ataca a distancia" es pasarle un FlyingMover
// y un RangedAttack — no hace falta ninguna clase nueva (ver EnemyFactory).
public class EnemyController : IDamageable
{
    public EnemyStats Stats { get; }
    public Vector3D Position { get; private set; }
    public IAttacker Attacker { get; private set; }

    private readonly IMover _mover;
    private readonly IAttacker? _enrageAttacker;
    private readonly float _enrageHealthFraction;
    private float _cooldownTimer;

    public int Health => Stats.Health;
    public bool IsDead => Stats.Health <= 0;

    public EnemyController(EnemyStats stats, Vector3D position, IMover mover, IAttacker attacker, IAttacker? enrageAttacker = null, float enrageHealthFraction = 0f)
    {
        Stats = stats;
        Position = position;
        _mover = mover;
        Attacker = attacker;
        _enrageAttacker = enrageAttacker;
        _enrageHealthFraction = enrageHealthFraction;
    }

    public void TakeDamage(int amount)
    {
        Stats.Health = Math.Max(0, Stats.Health - amount);
    }

    public void Tick(Vector3D targetPosition, IDamageable target, float deltaTime)
    {
        if (IsDead)
        {
            return;
        }

        // Cambio de comportamiento en tiempo de ejecución vía polimorfismo:
        // reasignamos el IAttacker sin condicionales por tipo ni herencia nueva.
        if (_enrageAttacker != null && Attacker != _enrageAttacker && Stats.Health <= Stats.MaxHealth * _enrageHealthFraction)
        {
            Attacker = _enrageAttacker;
        }

        var distance = Vector3D.Distance(Position, targetPosition);
        if (distance > Stats.AttackRange)
        {
            Position = _mover.Move(Position, targetPosition, Stats.MoveSpeed, deltaTime);
            return;
        }

        Attacker.TryAttack(Stats, Position, target, targetPosition, deltaTime, ref _cooldownTimer);
    }
}
