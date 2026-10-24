using UnityEngine;

// Damage formulas, kept out of MonoBehaviours so they can be unit tested
public static class DamageCalculator
{
    // Each point of armor reduces damage less: 100 armor halves it. Always at least 1 damage.
    public static int ApplyArmor(int damage, int armor)
    {
        float reductionFactor = 100f / (armor + 100f);
        return Mathf.Max(1, Mathf.RoundToInt(damage * reductionFactor));
    }

    // roll is a random value in [0, 1]; it crits when roll <= critChance (and critChance > 0)
    public static int RollAttackDamage(int attackDamage, float critChance, float critDamage, float roll, out bool isCrit)
    {
        isCrit = critChance > 0 && roll <= critChance;

        return isCrit ? Mathf.RoundToInt(attackDamage * critDamage) : attackDamage;
    }
}
