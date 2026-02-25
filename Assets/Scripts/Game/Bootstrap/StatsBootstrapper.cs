using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsBootstrapper
{
    private Dictionary<string, List<TalentsToApply>> talentsByUnit;

    public IReadOnlyDictionary<string, List<TalentsToApply>> TalentsByUnit => talentsByUnit;

    public class TalentsToApply
    {
        public string unit;
        public int purcashed;
        public List<AppliedEffect> effects = new List<AppliedEffect>();
    }

    private List<TalentsToApply> talentsToApply;

    public void LoadInPlayerTalents()
    {
        talentsToApply = new List<TalentsToApply>();

        foreach (var kvp in SaveService.Instance.Current.Talents.Purchases)
        {
            string unit = kvp.Key.Split("_")[0].ToLowerInvariant();

            var talentDef = TalentService.Instance
                .playerTalentTree.GetTalentById(kvp.Key);

            if (talentDef.Type != TalentType.StatModifier) continue;

            TalentsToApply talent = new TalentsToApply
            {
                unit = unit,
                purcashed = kvp.Value,
                effects = new List<AppliedEffect>(talentDef.Effects)
            };

            talentsToApply.Add(talent);
        }
    }

    public void BuildTalentsByUnit()
    {
        talentsByUnit = new Dictionary<string, List<TalentsToApply>>();

        foreach (var talent in talentsToApply)
        {
            if (!talentsByUnit.TryGetValue(talent.unit, out var list))
            {
                list = new List<TalentsToApply>();
                talentsByUnit[talent.unit] = list;
            }

            list.Add(talent);
        }
    }

    public void LoadAndBuildTalents()
    {
        LoadInPlayerTalents();
        BuildTalentsByUnit();
    }

    public void LogTalentsToApply()
    {
        Debug.Log("=== Talents To Apply ===");

        foreach (var kvp in talentsByUnit)
        {
            Debug.Log($"Unit: {kvp.Key}");

            foreach (var talent in kvp.Value)
            {
                foreach (var effect in talent.effects)
                {
                    Debug.Log(
                        $"    Effect → Target: {effect.Target}, " +
                        $"Operation: {effect.Operation}, Value: {effect.Value}" +
                        $"  Purchased: {talent.purcashed}");
                }
            }
        }
    }
}
