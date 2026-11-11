using UnityEngine;

namespace Clase14
{
    /// <summary>Mueve al jugador con WASD o las flechas. DialogueNpc lo desactiva mientras se habla.</summary>
    public class PlayerMover : MonoBehaviour
    {
        [Min(0)] public float speed = 5f;
        [Tooltip("La mitad del área por la que se puede caminar.")]
        public Vector2 bounds = new Vector2(8f, 3.5f);

        void Awake()
        {
            if (GetComponent<SpriteRenderer>() == null)
                SpriteFactory.AddRenderer(gameObject, SpriteFactory.Circle, new Color(0.3f, 0.8f, 1f), 3);
        }

        void Update()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            var position = transform.position + Vector3.ClampMagnitude(input, 1f) * (speed * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, -bounds.x, bounds.x);
            position.y = Mathf.Clamp(position.y, -bounds.y, bounds.y);
            transform.position = position;
        }
    }
}
