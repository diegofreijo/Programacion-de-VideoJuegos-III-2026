namespace UnityStubs;

public static class UnityObject
{
    public static T Instantiate<T>(string name, Vector3 position) where T : MonoBehaviour, new()
    {
        var gameObject = new GameObject(name, position);
        var component = new T { gameObject = gameObject };
        component.Start();
        return component;
    }

    public static void Destroy(GameObject gameObject)
    {
        gameObject.isDestroyed = true;
    }
}
