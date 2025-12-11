using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

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
        string jsonPath = "Assets/Resources/Data/Talents.json";
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"JSON file not found at path: {jsonPath}");
            return;
        }

        string jsonText = File.ReadAllText(jsonPath);

        TalentDatabase db = JsonUtility.FromJson<TalentDatabase>(jsonText);
        if (db == null)
        {
            Debug.LogError("Failed to parse JSON.");
            return;
        }

        List<Talent> fighterTalents = db.talents.fighter;
        //List<Talent> rangerTalents = db.talents.ranger;
        //List<Talent> cavalryTalents = db.talents.cavalry;

        // 3. Check each iconId
        bool anyMissing = false;
        foreach (var unitType in new List<List<Talent>> { db.talents.fighter/*, db.talents.ranger, db.talents.cavalry */})
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

