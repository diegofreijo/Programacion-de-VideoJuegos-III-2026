namespace UnityStubs;

public abstract class MonoBehaviour
{
    public GameObject gameObject { get; internal set; } = null!;

    public Transform transform => gameObject.transform;

    public virtual void Start() { }

    public virtual void Update() { }
}
