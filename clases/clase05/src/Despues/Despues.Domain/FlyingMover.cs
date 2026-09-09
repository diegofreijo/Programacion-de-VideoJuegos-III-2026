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
        var hoverPoint = new Vector3D(targetPosition.X, targetPosition.Y + _flightHeight, targetPosition.Z);
        return Vector3D.MoveTowards(currentPosition, hoverPoint, speed * deltaTime);
    }
}
