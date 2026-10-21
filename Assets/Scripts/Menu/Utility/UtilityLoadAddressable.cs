using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public static class UtilityLoadAddressable
{
    private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> iconCache = new();

    private static Sprite placeholderSprite;
    private static bool placeholderLoading;

    private const string PLACEHOLDER_ADDRESS = "place_holder_icon";
    private const string PRELOAD_LABEL = "preload";
    // Icons referenced in data whose art doesn't exist yet; shown as the placeholder without trying to load.
    // DataValidationTests fails if an icon is missing and not listed here, or listed here but now exists.
    public static readonly HashSet<string> KnownMissingIcons = new()
    {
        "unlock_ability_minor",
        "unlock_unit_dragoon",
        "unlock_unit_sniper",
        "unlock_unit_templar",
        "unlock_unit_trapper",
    };


    public static async Task PreloadIcons()
    {
        var locationsHandle =
            Addressables.LoadResourceLocationsAsync(PRELOAD_LABEL);

        var locations = await locationsHandle.Task;

        var tasks = new List<Task>();

        foreach (var location in locations)
        {
            string key = RemoveResolutionSuffix(location.PrimaryKey);

            if (KnownMissingIcons.Contains(key))
                continue;

            if (iconCache.ContainsKey(key))
                continue;

            var handle = Addressables.LoadAssetAsync<Sprite>(key);

            iconCache[key] = handle;
            tasks.Add(handle.Task);
        }

        await Task.WhenAll(tasks);

        Addressables.Release(locationsHandle);
    }

    public static async Task PreloadPlaceholder()
    {
        if (placeholderSprite != null || placeholderLoading)
            return;

        placeholderLoading = true;

        try
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(PLACEHOLDER_ADDRESS);

            placeholderSprite = await handle.Task;

            if (placeholderSprite == null)
            {
                Debug.LogError("Failed to load placeholder icon (null result)");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load placeholder icon: {e}");
        }
        finally
        {
            placeholderLoading = false;
        }
    }

    public static void LoadAddressableIcon(string address, VisualElement target)
    {
        if (target == null || string.IsNullOrWhiteSpace(address))
            return;

        InternalLoad(address, target);
    }

    public static void LoadAddressableIcon(AssetReference assetReference, VisualElement target)
    {
        if (target == null || assetReference == null)
            return;

        string key = assetReference.RuntimeKey.ToString();

        InternalLoad(key, target);
    }

    private static void InternalLoad(string key, VisualElement target)
    {
        if (KnownMissingIcons.Contains(key))
        {
            target.style.backgroundImage =
                new StyleBackground(placeholderSprite);
            return;
        }

        // CACHE HIT
        if (iconCache.TryGetValue(key, out AsyncOperationHandle<Sprite> cachedHandle))
        {
            // Handle became invalid somehow
            if (!cachedHandle.IsValid())
            {
                iconCache.Remove(key);
            }
            else
            {
                // Already loaded
                if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    target.style.backgroundImage =
                        new StyleBackground(cachedHandle.Result);

                    return;
                }

                // Still loading
                if (!cachedHandle.IsDone)
                {
                    cachedHandle.Completed += op =>
                    {
                        if (target == null)
                            return;

                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            target.style.backgroundImage =
                                new StyleBackground(op.Result);
                        }
                    };

                    return;
                }

                // Failed previously
                if (cachedHandle.Status == AsyncOperationStatus.Failed)
                {
                    iconCache.Remove(key);
                }
            }
        }

        // CACHE MISS
        AsyncOperationHandle<Sprite> handle =
            Addressables.LoadAssetAsync<Sprite>(key);

        iconCache[key] = handle;

        handle.Completed += op =>
        {
            if (target == null)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage =
                    new StyleBackground(op.Result);
            }
            else
            {
                iconCache.Remove(key);

                Debug.LogWarning($"Failed to load addressable sprite: {key}");
            }
        };

        //Apply placeholder
        if (placeholderSprite != null)
        {
            target.style.backgroundImage =
                new StyleBackground(placeholderSprite);
        }
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

    public static void ReleaseAll()
    {
        foreach (var handle in iconCache.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        iconCache.Clear();
    }
}