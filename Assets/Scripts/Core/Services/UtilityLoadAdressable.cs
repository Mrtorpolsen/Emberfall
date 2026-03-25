using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

public static class UtilityLoadAdressable
{
    private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> iconCache = new();

    private static Sprite placeholderSprite;
    private static bool placeholderLoading;

    public static async Task PreloadPlaceholder()
    {
        // Already loaded or loading → just return
        if (placeholderSprite != null || placeholderLoading)
            return;

        placeholderLoading = true;

        try
        {
            // Load the asset asynchronously
            var handle = Addressables.LoadAssetAsync<Sprite>("place_holder_icon");
            placeholderSprite = await handle.Task; // await instead of using Completed event

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
