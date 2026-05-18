using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text unitText; 
    [SerializeField] private TMP_Text cooldownText; 

    private AsyncOperationHandle<Sprite>? iconHandle;

    private float cooldown;
    private string cooldownKey;
    private Coroutine cooldownRoutine;

    private Action clickAction;
    private Func<bool> canInteract;

    public void Setup(string title, float cost, AssetReference icon, float cooldown, Func<bool> canInteractFunc)
    {
        unitText.text = title;
        costText.text = cost.ToString();
        canInteract = canInteractFunc;

        cooldownKey = title;

        LoadIcon(icon);

        if(cooldown > 0 && AbilityCooldownManager.Instance != null)
        {
            this.cooldown = cooldown;
            AbilityCooldownManager.Instance.OnCooldownTriggered += StartCooldown;
        }
        //Ensure cooldown text is hidden if not on cooldown
        if(cooldownText != null)
            cooldownText.text = "";
    }

    public void Setup(string title, float cost, Sprite icon, float cooldown, Func<bool> canInteractFunc)
    {
        unitText.text = title;
        costText.text = cost.ToString();
        canInteract = canInteractFunc;
        iconImage.sprite = icon;

        if (cooldown > 0 && AbilityCooldownManager.Instance != null)
        {
            this.cooldown = cooldown;
            AbilityCooldownManager.Instance.OnCooldownTriggered += StartCooldown;
        }
        //Ensure cooldown text is hidden if not on cooldown
        if (cooldownText != null)
            cooldownText.text = "";
    }

    public void UpdateText(string title, float cost)
    {
        unitText.text = title;
        costText.text = cost.ToString();
    }
    public void UpdateText(string title, string cost)
    {
        unitText.text = title;
        costText.text = cost;
    }

    public void OnClick()
    {
        clickAction?.Invoke();
    }

    public void SetClickAction(Action action)
    {
        clickAction = action;
    }

    private void LoadIcon(AssetReference iconReference)
    {
        if (iconReference == null || !iconReference.RuntimeKeyIsValid())
        {
            ReleaseIcon();
            iconImage.sprite = null;
            return;
        }

        // Prevent unnecessary loading if the same handler is already loaded
        if (iconReference.OperationHandle.IsValid())
        {
            iconImage.sprite = iconReference.OperationHandle.Convert<Sprite>().Result;
            return;
        }

        ReleaseIcon();

        var handle = iconReference.LoadAssetAsync<Sprite>();
        iconHandle = handle;

        handle.Completed += op =>
        {
            if (!this || !iconImage)
                return;

            if (op.Status == AsyncOperationStatus.Succeeded)
                iconImage.sprite = op.Result;
        };
    }

    private void StartCooldown(string abilityName)
    {
        if (cooldownText == null || this.cooldownKey != abilityName)
            return;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownText.gameObject.SetActive(true);

        iconImage.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        costText.rectTransform.localScale = new Vector3(0f, 1f, 1f);

        cooldownRoutine = StartCoroutine(CooldownRoutine(abilityName));
    }

    private IEnumerator CooldownRoutine(string abilityName)
    {
        while (true)
        {
            float remaining = AbilityCooldownManager.Instance.GetRemainingCooldown(abilityName, cooldown);

            if (remaining <= 0)
            {
                Refresh();
                break;
            }

            cooldownText.text = Mathf.Ceil(remaining).ToString();

            yield return null;
        }
        cooldownText.text = "";
        iconImage.rectTransform.localScale = new Vector3(1f, 1f, 1f);
        costText.rectTransform.localScale = new Vector3(1f, 1f, 1f);
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
        if (AbilityCooldownManager.Instance != null)
        {
            AbilityCooldownManager.Instance.OnCooldownTriggered -= StartCooldown;
        }
    }
}
