using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public static class UtilityLoadAdressable
{
    private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> iconCache = new();

    private static Sprite placeholderSprite;
    private static bool placeholderLoading;

    public static void PreloadPlaceholder()
    {
        if (placeholderSprite != null || placeholderLoading)
            return;

        placeholderLoading = true;

        var handle = Addressables.LoadAssetAsync<Sprite>("place_holder_icon");
        handle.Completed += op =>
        {
            placeholderLoading = false;

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                placeholderSprite = op.Result;
            }
            else
            {
                Debug.LogError("Failed to load placeholder icon");
            }
        };
    }

    public static void LoadAdressableIcon(string address, VisualElement target)
    {
        if (target == null)
            return;

        // Apply placeholder
        if (placeholderSprite != null)
        {
            target.style.backgroundImage = new StyleBackground(placeholderSprite);
        }

        // Cache hit
        if (iconCache.TryGetValue(address, out var cachedHandle))
        {
            if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage = new StyleBackground(cachedHandle.Result);
            }
            return;
        }

        // Cache miss
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        iconCache[address] = handle;

        handle.Completed += op =>
        {
            if (target == null)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage = new StyleBackground(op.Result);
            }
        };
    }

    public static void ReleaseAll()
    {
        foreach (var handle in iconCache.Values)
        {
            Addressables.Release(handle);
        }

        iconCache.Clear();
    }
}
