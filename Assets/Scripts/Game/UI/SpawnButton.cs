using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

[RequireComponent(typeof(Button))]
public class SpawnButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text unitText; 
    [SerializeField] private SpawnSide spawnSide;

    private AsyncOperationHandle<Sprite>? iconHandle;

    private System.Action clickAction;

    private float cost;

    public SpawnSide SpawnSide => spawnSide;

    public void Setup(SpawnDefinition def)
    {
        unitText.text = def.DisplayName;
        costText.text = def.Cost.ToString();

        cost = def.Cost;

        LoadIcon(def.Icon);
    }

    public void SetClickAction(System.Action action)
    {
        clickAction = action;
    }

    private void LoadIcon(AssetReference iconReference)
    {
        ReleaseIcon();

        if (iconReference == null || !iconReference.RuntimeKeyIsValid())
        {
            iconImage.sprite = null;
            return;
        }

        var handle = iconReference.LoadAssetAsync<Sprite>();
        iconHandle = handle;

        handle.Completed += op =>
        {
            // Button may have been destroyed or reused
            if (!this || !iconImage)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
                iconImage.sprite = op.Result;
        };
    }

    private void ReleaseIcon()
    {
        if (iconHandle.HasValue)
        {
            Addressables.Release(iconHandle.Value);
            iconHandle = null;
        }
    }

    public void OnClick()
    {
        clickAction?.Invoke();
    }

    public void SetInteractable(float playerCurrency,  bool isPaused)
    {
        button.interactable = (!isPaused && playerCurrency >= cost);
    }

    private void OnDestroy()
    {
        ReleaseIcon();
    }
}
