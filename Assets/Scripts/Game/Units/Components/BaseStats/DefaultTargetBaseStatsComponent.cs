using UnityEngine;

public class DefaultTarget : MonoBehaviour, ITargetable
{
    [Header("Reference")]
    [SerializeField] public GameObject unit;

    [Header("Attributes")]
    [SerializeField] public int maxHealth = 1;
    [SerializeField] public int currentHealth;
    [SerializeField] private float hitRadius = 0.1f;


    public Team Team { get; set; }

    public GameObject GameObject => unit;

    public Transform Transform => (this != null) ? gameObject.transform : null;

    public float HitRadius => hitRadius;

    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException();
    }

    void Die()
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransform()
    {
        return (this != null) ? gameObject.transform : null;
    }

    void ITargetable.Die()
    {
        throw new System.NotImplementedException();
    }
}
