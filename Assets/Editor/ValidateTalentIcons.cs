using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ValidateTalentIcons
{
    [MenuItem("Tools/Validate Talent Icons")]
    public static void ValidateIcons()
    {
        // 1. Load all Addressable addresses
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables Settings not found.");
            return;
        }

        HashSet<string> addresses = new HashSet<string>();
        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                addresses.Add(entry.address);
            }
        }

        Debug.Log($"Found {addresses.Count} Addressable addresses.");

        // 2. Load upgrade JSON
        string talentJsonAddress = "Talents"; // <-- Use the Addressable address you set for Talents.json

        Addressables.LoadAssetAsync<TextAsset>(talentJsonAddress).Completed += handle =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load JSON via Addressables at address: {talentJsonAddress}");
                return;
            }

            string jsonText = handle.Result.text;

            TalentDatabase db;
            try
            {
                db = JsonUtility.FromJson<TalentDatabase>(jsonText);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse JSON: {e.Message}");
                return;
            }

            // 3. Validate icon IDs
            bool anyMissing = false;
            foreach (var unitType in new List<List<Talent>> { db.talents.fighter/*, db.talents.ranger, db.talents.cavalry*/ })
            {
                foreach (var talent in unitType)
                {
                    if (!addresses.Contains(talent.IconId))
                    {
                        Debug.LogError($"Missing Addressable for iconId: {talent.IconId}");
                        anyMissing = true;
                    }
                }
            }

            if (!anyMissing)
                Debug.Log("All iconIds match Addressables!");
        };
    }
}

[System.Serializable]
public class TalentDatabase
{
    public TalentDictionary talents;
}

[System.Serializable]
public class TalentDictionary
{
    public List<Talent> fighter;
    public List<Talent> ranger;
    public List<Talent> cavalry;
    // Add new units here as needed
}

[System.Serializable]
public class Talent
{
    public string Id;
    public string IconId;
}

