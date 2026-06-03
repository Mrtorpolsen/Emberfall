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
        // Apply placeholder immediately
        if (placeholderSprite != null)
        {
            target.style.backgroundImage =
                new StyleBackground(placeholderSprite);
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

                Debug.LogError($"Failed to load addressable sprite: {key}");
            }
        };
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