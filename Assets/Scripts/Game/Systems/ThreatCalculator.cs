using UnityEngine;

public class ThreatCalculator
{
    private float rangeCoefficient = 0.2f; // Adjust this value to change the influence of range on threat calculation
    private float movementCoefficient = 0.03f; // Adjust this value to change the influence of movement on threat calculation

    //CritFactor= 1+ CritChance(CritMultiplier−1)
    //EDPH = Damage * CritFactor
    //DPS = AttackSpeed × EDPH(effect damager per hit) × AOEFactor(if present)
    //EHP(effective health) = HP^0.85 x ((armor + 100)/100)
    //Power = DPS * EHP * (1 + range * rangeCoefficient) * (1 + movement * movementCoefficient)
    //Threat = sqrt(Power)
    public float CalculateThreat(UnitStatsDefinition unitStats)
    {
        float edph = unitStats.attackDamage * (1 + unitStats.critChance * (unitStats.critDamage - 1));
        float dps = edph * unitStats.attackSpeed;
        float ehp = Mathf.Pow(unitStats.maxHealth, 0.85f) * ((unitStats.armor + 100) / 100f);
        float rangeFactor = 1 + unitStats.attackRange * rangeCoefficient;
        float movementFactor = 1 + unitStats.movementSpeed * movementCoefficient;

        return Mathf.Sqrt(dps * ehp * rangeFactor * movementFactor);
    }
}
