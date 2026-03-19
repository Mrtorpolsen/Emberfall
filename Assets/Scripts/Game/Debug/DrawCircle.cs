using UnityEngine;

public class DrawCircle : MonoBehaviour
{
    [SerializeField] public float radius;
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
