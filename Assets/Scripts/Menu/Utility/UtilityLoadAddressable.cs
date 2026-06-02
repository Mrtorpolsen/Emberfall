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

    public static async Task PreloadPlaceholder()
    {
        // Already loaded or loading → just return
        if (placeholderSprite != null || placeholderLoading)
            return;

        placeholderLoading = true;

        try
        {
            // Load the asset asynchronously
            var handle = Addressables.LoadAssetAsync<Sprite>(PLACEHOLDER_ADDRESS);
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
        // Apply placeholder immediately
        if (placeholderSprite != null)
        {
            target.style.backgroundImage =
                new StyleBackground(placeholderSprite);
        }

        // Cache hit
        if (iconCache.TryGetValue(key, out AsyncOperationHandle<Sprite> cachedHandle))
        {
            if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                target.style.backgroundImage =
                    new StyleBackground(cachedHandle.Result);
            }

            return;
        }

        // Cache miss
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
                Debug.LogError($"Failed to load addressable sprite: {key}");
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
