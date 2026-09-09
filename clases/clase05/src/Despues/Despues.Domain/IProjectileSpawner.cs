namespace Despues.Domain;

// Puerto: el dominio pide "lanzar un proyectil" sin saber cómo se representa
// visualmente. Cada entorno de ejecución trae su propia implementación
// (Despues.Unity y Despues.DomainConsole).
public interface IProjectileSpawner
{
    void Spawn(Vector3D from, Vector3D to, int damage, IDamageable target);
}
