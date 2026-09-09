namespace Despues.Domain;

public interface IMover
{
    Vector3D Move(Vector3D currentPosition, Vector3D targetPosition, float speed, float deltaTime);

    // El punto contra el que hay que medir el rango de ataque: para moverse por
    // el piso es la posición del objetivo; para volar es el punto de sobrevuelo
    // (ver FlyingMover). Sin esto, EnemyController mediría distancia al jugador
    // en vez de a donde el mover realmente lleva al enemigo.
    Vector3D GetAttackPoint(Vector3D targetPosition);
}
