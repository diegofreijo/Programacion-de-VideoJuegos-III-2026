namespace UnityStubs;

public class GameObject
{
    public string name;
    public Transform transform { get; }
    public bool isDestroyed;

    public GameObject(string name, Vector3 position = default)
    {
        this.name = name;
        transform = new Transform(position);
    }
}
