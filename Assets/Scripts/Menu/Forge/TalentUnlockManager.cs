using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentUnlockManager : MonoBehaviour
{
    public static TalentUnlockManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject if it doesn't exist yet
                GameObject go = new GameObject("TalentPointsManager");
                _instance = go.AddComponent<TalentUnlockManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    private static TalentUnlockManager _instance;

    private Dictionary<string, Dictionary<int, int>> pointsPerTierPerUnit
        = new Dictionary<string, Dictionary<int, int>>();

    public void InitializeFromForge()
    {
        var purchases = SaveService.Instance.Current.Talents.Purchases;

        pointsPerTierPerUnit.Clear();

        foreach (var kvp in purchases)
        {
            string unit = kvp.Key.Split("_")[0].ToLowerInvariant();
            var talentDef = TalentService.Instance.playerTalentTree.GetTalentById(kvp.Key);

            if (!pointsPerTierPerUnit.ContainsKey(unit))
            {
                pointsPerTierPerUnit[unit] = new Dictionary<int, int>();
            }

            if (!pointsPerTierPerUnit[unit].ContainsKey(talentDef.Tier))
            {
                pointsPerTierPerUnit[unit][talentDef.Tier] = 0;
            }

            pointsPerTierPerUnit[unit][talentDef.Tier] += kvp.Value;
        }
        DebugPrintPoints();
    }

    public void AddPoints(string unit, int tier, int amount)
    {
        if (!pointsPerTierPerUnit.ContainsKey(unit))
        {
            pointsPerTierPerUnit[unit] = new Dictionary<int, int>();
        }

        if (!pointsPerTierPerUnit[unit].ContainsKey(tier))
        {
            pointsPerTierPerUnit[unit][tier] = 0;
        }

        pointsPerTierPerUnit[unit][tier] += amount;
        DebugPrintPoints();
    }

    public bool ArePrerequisitesMet(string unit, TalentPrerequisite prerequisite)
    {
        // Check require points in previous tier
        if (prerequisite.RequiredTier > 0)
        {
            int pointsInTier = GetPointsInTier(unit, prerequisite.RequiredTier);
            if (pointsInTier < prerequisite.RequiredPointsInTier)
            {
                return false;
            }
        }

        // Check required talent
        if (!string.IsNullOrEmpty(prerequisite.RequiredUpgradeId))
        {
            bool hasUpgrade = SaveService.Instance.Current.Talents.Purchases
                .ContainsKey(prerequisite.RequiredUpgradeId);
            if (!hasUpgrade)
            {
                return false;
            }
        }

        // Check required achievement // Achievements not implemented yet
        //if (!string.IsNullOrEmpty(prerequisite.RequiredAchievementId))
        //{
        //    bool hasAchievement = AchievementManager.Instance.HasAchievement(prerequisite.RequiredAchievementId);
        //    if (!hasAchievement)
        //        return false;
        //}

        // All checks passed
        return true;
    }

    public bool ArePrerequisitesMet(string unit, TalentPrerequisite[] prerequisites)
    {
        if (prerequisites == null || prerequisites.Length == 0)
            return true;

        foreach (var prerequisite in prerequisites)
        {
            if (!ArePrerequisitesMet(unit, prerequisite))
                return false;
        }

        return true;
    }

    private int GetPointsInTier(string unitId, int tier)
    {
        if (!pointsPerTierPerUnit.TryGetValue(unitId, out var tierDict))
            return 0;

        if (!tierDict.TryGetValue(tier, out int points))
            return 0;

        return points;
    }

    public void ResetAll()
    {
        pointsPerTierPerUnit.Clear();
    }

    public void DebugPrintPoints()
    {
        if (pointsPerTierPerUnit.Count == 0)
        {
            Debug.Log("No talent points tracked yet.");
            return;
        }

        foreach (var unitKvp in pointsPerTierPerUnit)
        {
            string unitName = unitKvp.Key;
            var tierDict = unitKvp.Value;

            string tiersInfo = "";
            foreach (var tierKvp in tierDict)
            {
                tiersInfo += $"Tier {tierKvp.Key}: {tierKvp.Value} points, ";
            }

            // Remove trailing comma and space
            if (tiersInfo.Length > 2)
            {
                tiersInfo = tiersInfo.Substring(0, tiersInfo.Length - 2);
            }

        }
    }

}
