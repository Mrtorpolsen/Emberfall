using UnityEngine;

public class TowerUnitStats : MonoBehaviour
{
    [Header("Tower Tier Prefabs")]
    [SerializeField] private GameObject[] tierPrefabs;

    [Header("Tower Tier Settings")]
    [SerializeField] private int maxTier = 3;
    [SerializeField] private int currentTier = 1;

    private float baseCost;
    private float baseDamage;
    private float baseAttackSpeed;
    private float baseRange;

    public int CurrentTier => currentTier;
    public int MaxTier => maxTier;

    public float GetCost() => baseCost * Mathf.Pow(1.1f, currentTier - 1);

    public FinalStats GetTierStats()
    {
        float multiplier = Mathf.Pow(1.1f, currentTier - 1);

        return new FinalStats
        {
            attackDamage = Mathf.RoundToInt(baseDamage * multiplier),
            attackSpeed = baseAttackSpeed * multiplier,
            attackRange = baseRange * multiplier,
            cost = GetCost()
        };
    }

    public bool CanUpgrade() => currentTier < maxTier;

    public void UpgradeTier()
    {
        if (!CanUpgrade()) return;
        currentTier++;
    }

    public GameObject GetPrefabForTier()
    {
        if (tierPrefabs != null && tierPrefabs.Length >= currentTier && tierPrefabs[currentTier - 1] != null)
            return tierPrefabs[currentTier - 1];

        return gameObject;
    }
}
