using UnityEngine;

public class VelocityInspector : MonoBehaviour
{
    [SerializeField] private Vector2 currentVelocity;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        currentVelocity = rb.linearVelocity;
    }
}