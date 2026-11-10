using UnityEngine;

// Draws a radius gizmo on the selected object.
// Destroys itself in release builds.
public class DrawCircle : MonoBehaviour
{
    [SerializeField] public float radius;

    // Release builds: remove the component (disabled first so Update never runs).
    void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            enabled = false;
            Destroy(this);
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
