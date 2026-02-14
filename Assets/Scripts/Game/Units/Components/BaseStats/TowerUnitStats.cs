using UnityEngine;

public class TowerUnitStats : BaseUnitStats
{
    [Header("Tower Tier Settings")]
    [SerializeField] private int maxTier = 5;
    [SerializeField] private int currentTier = 1;

    [Header("Tower Tier Stars")]
    [SerializeField] private GameObject[] tierStars;

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
        float multiplier = Mathf.Pow(TierMultiplier, currentTier - 1);

        return new FinalStats
        {
            attackDamage = Mathf.RoundToInt(BaseStats.attackDamage * multiplier),
            attackSpeed = BaseStats.attackSpeed * multiplier,
            attackRange = BaseStats.attackRange * multiplier,
        };
    }

    public void UpgradeTier()
    {
        if (!CanUpgrade()) return;

        currentTier++;

        FinalStats newStats = GetTierStats();
        ApplyFinalStats(newStats);

        ShowTierStars();
    }

    public void ShowTierStars()
    {
        for (int i = 0; i < currentTier; i++)
        {
            tierStars[i].SetActive(true);
        }
    }
}
