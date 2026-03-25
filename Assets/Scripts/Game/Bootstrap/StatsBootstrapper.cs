using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsBootstrapper
{
    private Dictionary<string, List<TalentsToApply>> talentsByUnit;
    private Dictionary<ResearchCategory, List<ResearchToApply>> researchByCategory;

    public IReadOnlyDictionary<string, List<TalentsToApply>> TalentsByUnit => talentsByUnit;
    public IReadOnlyDictionary<ResearchCategory, List<ResearchToApply>> ResearchByCategory => researchByCategory;

    public class TalentsToApply
    {
        public string unit;
        public int purchased;
        public List<StatEffect> effects = new List<StatEffect>();
    }

    public class ResearchToApply
    {
        public ResearchCategory category;
        public int purchased;
        public List<StatEffect> effects = new();
    }

    private List<TalentsToApply> talentsToApply;
    private List<ResearchToApply> researchToApply;

    public void LoadInPlayerTalents()
    {
        talentsToApply = new List<TalentsToApply>();

        foreach (var kvp in SaveService.Instance.Current.Talents.Purchases)
        {
            string unit = kvp.Key.Split("_")[0].ToLowerInvariant();

            TalentDefinition talentDef = TalentService.Instance
                .playerTalentTree.GetTalentById(kvp.Key);

            if (talentDef.Type != TalentType.StatModifier) continue;

            TalentsToApply talent = new TalentsToApply
            {
                unit = unit,
                purchased = kvp.Value,
                effects = new List<StatEffect>(talentDef.Effects)
            };

            talentsToApply.Add(talent);
        }
    }

    public void LoadInPlayerResearch()
    {
        researchToApply = new List<ResearchToApply>();

        foreach (var kvp in SaveService.Instance.Current.Research.CompletedResearch)
        {
            ResearchDefinition researchDef = ResearchService.Instance.playerResearchTree.GetResearchById(kvp.Key);

            //If add category type, skip non stats.
            // TODO get only stat related research, need to move it to its own and only expose stat related here.

            if (researchDef.Category == ResearchCategory.GlobalAbility || researchDef.Category == ResearchCategory.Economy)
                continue;

            ResearchToApply research = new ResearchToApply
            {
                category = researchDef.Category,
                purchased = kvp.Value,
                effects = new List<StatEffect>(researchDef.Effects)
            };

            researchToApply.Add(research);
        }
    }

    public void BuildTalentsByUnit()
    {
        talentsByUnit = new Dictionary<string, List<TalentsToApply>>();

        foreach (TalentsToApply talent in talentsToApply)
        {
            if (!talentsByUnit.TryGetValue(talent.unit, out var list))
            {
                list = new List<TalentsToApply>();
                talentsByUnit[talent.unit] = list;
            }

            list.Add(talent);
        }
    }

    public void BuildResearchByCategory()
    {
        researchByCategory = new Dictionary<ResearchCategory, List<ResearchToApply>>();

        foreach (ResearchToApply research in researchToApply)
        {
            if (!researchByCategory.TryGetValue(research.category, out var list))
            {
                list = new List<ResearchToApply>();
                researchByCategory[research.category] = list;
            }

            list.Add(research);
        }
    }

    public void LoadAndBuildTalents()
    {
        LoadInPlayerTalents();
        BuildTalentsByUnit();
    }
    public void LoadAndBuildResearch()
    {
        LoadInPlayerResearch();
        BuildResearchByCategory();
    }

    public void ClearTalents()
    {
        talentsByUnit?.Clear();
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
                        $"  Purchased: {talent.purchased}");
                }
            }
        }
    }
}
