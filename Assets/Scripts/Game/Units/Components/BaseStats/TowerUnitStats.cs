using UnityEngine;

public class TowerUnitStats : BaseUnitStats
{
    [Header("Tower Tier Prefabs")]
    [SerializeField] private GameObject[] tierPrefabs;

    [Header("Tower Tier Settings")]
    [SerializeField] private int maxTier = 5;
    [SerializeField] private int currentTier = 1;

    //private float baseCost;
    //private float baseDamage;
    //private float baseAttackSpeed;
    //private float baseRange;
    public bool CanUpgrade() => currentTier < maxTier;

    private const float TierMultiplier = 1.1f;
    public int CurrentTier => currentTier;
    public int MaxTier => maxTier;


    public float GetUpgradeCost()
    {
        return Mathf.RoundToInt(Cost * Mathf.Pow(TierMultiplier, currentTier));
    }

    public float GetSellValue()
    {
        return Mathf.RoundToInt(GetTotalInvested() * 0.5f);
    }

    public float GetTotalInvested()
    {
        float total = 0f;

        for (int i = 0; i < currentTier; i++)
        {
            total += Cost * Mathf.Pow(TierMultiplier, i);
        }

        return total;
    }

    public FinalStats GetTierStats()
    {
        float multiplier = Mathf.Pow(1.1f, currentTier - 1);

        return new FinalStats
        {
            attackDamage = Mathf.RoundToInt(AttackDamage * multiplier),
            attackSpeed = AttackSpeed * multiplier,
            attackRange = AttackRange * multiplier,
            cost = GetUpgradeCost()
        };
    }

    public void UpgradeTier()
    {
        if (!CanUpgrade()) return;
        currentTier++;
    }

    public GameObject GetPrefabForTier()
    {
        if (tierPrefabs != null && currentTier > 0 && currentTier <= tierPrefabs.Length && tierPrefabs[currentTier - 1] != null)
        {
            return tierPrefabs[currentTier - 1];
        }

        if (tierPrefabs != null && tierPrefabs.Length > 0 && tierPrefabs[0] != null)
        {
            Debug.LogWarning($"Tower {name} tier {currentTier} has no prefab assigned! Using tier 1 as fallback.");
            return tierPrefabs[0];
        }

        Debug.LogError($"Tower {name} has no prefabs assigned at all!");
        return null;
    }
}
