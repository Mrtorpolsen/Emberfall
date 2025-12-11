using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class TalentIconAddressSetup
{
    [MenuItem("Tools/Update Talent Icon Addresses")]
    public static void UpdateAddresses()
    {
        string folderPath = "Assets/Art/TalentIcons"; // Adjust to your folder
        string[] files = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressables Settings not found.");
            return;
        }

        foreach (string file in files)
        {
            string assetPath = file.Replace("\\", "/"); // ensure consistent path
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            // Remove resolution suffix (_512x512, _256x256, etc.)
            string cleanedName = RemoveResolutionSuffix(fileName);

            // Set or create Addressable entry
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), settings.DefaultGroup);
            }

            entry.address = cleanedName;

            Debug.Log($"Set Addressable Address: {assetPath} -> {cleanedName}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Talent icon addresses updated.");
    }

    private static string RemoveResolutionSuffix(string fileName)
    {
        // Example: melee_damage_1_512x512 -> melee_damage_1
        int lastUnderscore = fileName.LastIndexOf("_");
        if (lastUnderscore < 0) return fileName;

        string possibleRes = fileName.Substring(lastUnderscore + 1);
        if (possibleRes.Contains("x")) // crude check for resolution pattern
        {
            return fileName.Substring(0, lastUnderscore);
        }

        return fileName;
    }
}
