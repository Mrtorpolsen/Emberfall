using UnityEngine;

// Shows the Rigidbody2D velocity in the Inspector. On most unit prefabs.
// Destroys itself in release builds, so the prefabs can keep it.
public class VelocityInspector : MonoBehaviour
{
    [SerializeField] private Vector2 currentVelocity;

    private Rigidbody2D rb;

    // Release builds: remove the component (disabled first so Update never runs).
    void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            enabled = false;
            Destroy(this);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        currentVelocity = rb.linearVelocity;
    }
}
