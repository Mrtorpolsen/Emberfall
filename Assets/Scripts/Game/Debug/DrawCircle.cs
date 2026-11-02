using UnityEngine;

// Draws a radius gizmo on the selected object.
// Release builds compile only a stub that destroys itself.
public class DrawCircle : MonoBehaviour
{
    [SerializeField] public float radius;

#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
    void Awake()
    {
        Destroy(this);
    }
#else
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
