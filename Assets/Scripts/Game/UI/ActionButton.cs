using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text unitText; 

    private AsyncOperationHandle<Sprite>? iconHandle;

    private System.Action clickAction;
    private System.Func<bool> canInteract;

    public void Setup(string title, float cost, AssetReference icon, System.Func<bool> canInteractFunc)
    {
        unitText.text = title;
        costText.text = cost.ToString();
        canInteract = canInteractFunc;

        LoadIcon(icon);
    }
    public void Setup(string title, float cost, Sprite icon, System.Func<bool> canInteractFunc)
    {
        unitText.text = title;
        costText.text = cost.ToString();
        canInteract = canInteractFunc;
        iconImage.sprite = icon;
    }

    public void UpdateText(string title, float cost)
    {
        unitText.text = title;
        costText.text = cost.ToString();
    }
    public void UpdateText(string title, string cost)
    {
        unitText.text = title;
        costText.text = cost.ToString();
    }

    public void OnClick()
    {
        clickAction?.Invoke();
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
            {
                iconImage.sprite = op.Result;
            }
        };
    }

    public void Refresh()
    {
        button.interactable = canInteract == null || canInteract();
    }

    private void ReleaseIcon()
    {
        if (iconHandle.HasValue)
        {
            Addressables.Release(iconHandle.Value);
            iconHandle = null;
        }
    }

    private void OnDestroy()
    {
        ReleaseIcon();
    }
}
