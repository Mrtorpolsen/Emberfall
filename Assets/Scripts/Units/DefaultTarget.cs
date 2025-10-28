using UnityEngine;

public class DefaultTarget : MonoBehaviour, ITargetable
{
    [Header("Reference")]
    [SerializeField] public GameObject target;

    [Header("Attributes")]
    [SerializeField] public int maxHealth = 1;
    [SerializeField] public int currentHealth;
    [SerializeField] private float hitRadius = 0.1f;


    public Team Team { get; set; }
    public GameObject GetGameObject() => target;
    public Team GetTeam() => Team;
    public bool GetIsAlive() => currentHealth > 0;
    public float GetHitRadius() => hitRadius;

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
