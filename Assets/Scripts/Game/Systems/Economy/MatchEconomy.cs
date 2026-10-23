using System;
using System.Collections.Generic;

// A match's currency per team and the player's income. Plain C# so it can be unit tested;
// GameManager owns it and the UI listens to its events instead of being called directly.
public class MatchEconomy
{
    private const float IncomeMultiplierStep = 0.2f;
    private const float IncomeUpgradeCostStep = 50f;

    private readonly Dictionary<Team, float> currency;
    private readonly float baseIncomePerTick;
    private readonly float incomeInterval;
    private float incomeTimer;

    public float IncomeMultiplier { get; private set; }
    public float IncomeUpgradeCost { get; private set; }
    public float IncomePerTick => baseIncomePerTick * IncomeMultiplier;

    public event Action<Team, float> CurrencyChanged;
    public event Action IncomeChanged;

    public MatchEconomy(float southCurrency, float northCurrency, float baseIncomePerTick, float incomeInterval,
        float incomeMultiplier, float incomeUpgradeCost)
    {
        currency = new Dictionary<Team, float>
        {
            { Team.South, southCurrency },
            { Team.North, northCurrency },
        };

        this.baseIncomePerTick = baseIncomePerTick;
        this.incomeInterval = incomeInterval;
        IncomeMultiplier = incomeMultiplier;
        IncomeUpgradeCost = incomeUpgradeCost;
    }

    public float Get(Team team) => currency[team];

    public bool CanAfford(Team team, float amount) => currency[team] >= amount;

    public void Add(Team team, float amount)
    {
        currency[team] += amount;
        CurrencyChanged?.Invoke(team, currency[team]);
    }

    // Checks and spends in one call so a purchase can never take currency below zero
    public bool TrySpend(Team team, float amount)
    {
        if (!CanAfford(team, amount))
            return false;

        currency[team] -= amount;
        CurrencyChanged?.Invoke(team, currency[team]);
        return true;
    }

    // Pays the player's income once per interval; call every frame while the match runs
    public void Tick(float deltaTime)
    {
        incomeTimer += deltaTime;

        if (incomeTimer < incomeInterval)
            return;

        incomeTimer = 0f;
        Add(Team.South, IncomePerTick);
    }

    public bool TryUpgradeIncome()
    {
        if (!TrySpend(Team.South, IncomeUpgradeCost))
            return false;

        IncomeMultiplier += IncomeMultiplierStep;
        IncomeUpgradeCost += IncomeUpgradeCostStep;
        IncomeChanged?.Invoke();
        return true;
    }
}
