namespace Despues.Domain;

public class GroundMover : IMover
{
    public Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime)
    {
        return Vector3D.MoveTowards(currentPosition, targetPosition, speed * deltaTime);
    }
}
