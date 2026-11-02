using UnityEngine;

// Shows the Rigidbody2D velocity in the Inspector. On most unit prefabs.
// Release builds compile only a stub that destroys itself, so the prefabs can keep it.
public class VelocityInspector : MonoBehaviour
{
    [SerializeField] private Vector2 currentVelocity;

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
    void Awake()
    {
        Destroy(this);
    }
#else
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        currentVelocity = rb.linearVelocity;
    }
#endif
}
