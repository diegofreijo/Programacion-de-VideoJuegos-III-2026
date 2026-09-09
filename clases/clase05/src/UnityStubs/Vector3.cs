namespace UnityStubs;

public struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        var dz = a.z - b.z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        var dx = target.x - current.x;
        var dy = target.y - current.y;
        var dz = target.z - current.z;
        var distance = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

        if (distance <= maxDistanceDelta || distance == 0f)
        {
            return target;
        }

        var t = maxDistanceDelta / distance;
        return new Vector3(current.x + dx * t, current.y + dy * t, current.z + dz * t);
    }

    public override string ToString() => $"({x:0.0}, {y:0.0}, {z:0.0})";
}
