namespace Despues.Domain;

public class FlyingMover : IMover
{
    private readonly float _flightHeight;

    public FlyingMover(float flightHeight)
    {
        _flightHeight = flightHeight;
    }

    public Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime)
    {
        return Vector3D.MoveTowards(currentPosition, GetAttackPoint(targetPosition), speed * deltaTime);
    }

    public Vector3D GetAttackPoint(Vector3D targetPosition) =>
        new(targetPosition.X, targetPosition.Y + _flightHeight, targetPosition.Z);
}
