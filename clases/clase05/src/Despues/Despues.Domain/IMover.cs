namespace Despues.Domain;

public interface IMover
{
    Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime);
}
