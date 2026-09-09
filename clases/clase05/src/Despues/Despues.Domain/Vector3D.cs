namespace Despues.Domain;

// Vector propio del dominio: distinto de UnityStubs.Vector3 a propósito.
// El dominio no conoce el vector del motor; el adaptador de Unity (Despues.Unity)
// es el que convierte entre uno y otro.
public struct Vector3D
{
    public float X;
    public float Y;
    public float Z;

    public Vector3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static float Distance(Vector3D a, Vector3D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static Vector3D MoveTowards(Vector3D current, Vector3D target, float maxDistanceDelta)
    {
        var dx = target.X - current.X;
        var dy = target.Y - current.Y;
        var dz = target.Z - current.Z;
        var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance <= maxDistanceDelta || distance == 0f)
        {
            return target;
        }

        var t = maxDistanceDelta / distance;
        return new Vector3D(current.X + dx * t, current.Y + dy * t, current.Z + dz * t);
    }

    public override string ToString() => $"({X:0.0}, {Y:0.0}, {Z:0.0})";
}
