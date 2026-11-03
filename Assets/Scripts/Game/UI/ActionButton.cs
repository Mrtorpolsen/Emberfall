using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text topText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text bottomText;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Hold")]
    [Tooltip("Seconds the button must be held before the hold action starts repeating")]
    [SerializeField] private float holdDelay = 0.4f;
    [Tooltip("Seconds between repeats while held")]
    [SerializeField] private float holdRepeatInterval = 0.25f;

    private AsyncOperationHandle<Sprite>? iconHandle;

    private string cooldownKey;
    private Coroutine cooldownRoutine;

    private Action clickAction;
    private Func<bool> canInteract;

    private Action holdAction;
    private Coroutine holdRoutine;
    private int holdPointerId;
    // Set once a hold has fired, so releasing doesn't also trigger the normal click
    private bool suppressClick;

    public void Setup(string title, float cost, AssetReference icon, float cooldown, Func<bool> canInteractFunc)
    {
        topText.text = cost.ToString();
        bottomText.text = title;
        canInteract = canInteractFunc;

        LoadIcon(icon);

        //Ensure cooldown text is hidden if not on cooldown
        if (cooldownText != null)
            cooldownText.text = "";
    }

    public void Setup(string title, float cost, Sprite icon, float cooldown, Func<bool> canInteractFunc)
    {
        bottomText.text = title;
        topText.text = cost.ToString();
        canInteract = canInteractFunc;
        iconImage.sprite = icon;

        //Ensure cooldown text is hidden if not on cooldown
        if (cooldownText != null)
            cooldownText.text = "";
    }

    public void Setup(AbilityDefinition ability, Func<bool> canInteractFunc)
    {
        bottomText.text = ability.name;
        topText.text = ability.Cost.ToString();
        canInteract = canInteractFunc;
        LoadIcon(ability.Icon);

        if (AbilityCooldownManager.Instance != null)
        {
            cooldownKey = AbilityCooldownManager.Instance.GetCooldownKey(ability);

            if (ability.cooldown > 0)
            {
                AbilityCooldownManager.Instance.OnCooldownTriggered += StartCooldown;
            }
        }

        //Ensure cooldown text is hidden if not on cooldown
        if (cooldownText != null)
            cooldownText.text = "";
    }


    public void UpdateText(string title, float cost)
    {
        bottomText.text = title;
        topText.text = cost.ToString();
    }
    public void UpdateText(string title, string cost)
    {
        bottomText.text = title;
        topText.text = cost;
    }

    public void OnClick()
    {
        if (suppressClick)
        {
            suppressClick = false;
            return;
        }

        clickAction?.Invoke();
    }

    public void SetClickAction(Action action)
    {
        clickAction = action;
    }

    // Repeats while the button is held; a short tap still only runs the click action
    public void SetHoldAction(Action action)
    {
        holdAction = action;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        suppressClick = false;

        if (holdAction == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        StopHold();
        holdPointerId = eventData.pointerId;
        holdRoutine = StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == holdPointerId)
            StopHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerId == holdPointerId)
            StopHold();
    }

    // Realtime so the repeat rate doesn't depend on timeScale; canInteract already blocks it while paused
    private IEnumerator HoldRoutine()
    {
        yield return new WaitForSecondsRealtime(holdDelay);

        suppressClick = true;

        while (true)
        {
            // Keeps waiting while unaffordable, so holding sends again as soon as there's money
            if (canInteract == null || canInteract())
                holdAction?.Invoke();

            yield return new WaitForSecondsRealtime(holdRepeatInterval);
        }
    }

    private void StopHold()
    {
        if (holdRoutine != null)
        {
            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }
    }

    private void OnDisable()
    {
        StopHold();
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

    private void StartCooldown(string abilityKey)
    {
        if (cooldownText == null || this.cooldownKey != abilityKey)
            return;

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownText.gameObject.SetActive(true);

        iconImage.rectTransform.localScale = new Vector3(0f, 1f, 1f);
        topText.rectTransform.localScale = new Vector3(0f, 1f, 1f);

        cooldownRoutine = StartCoroutine(CooldownRoutine(cooldownKey));
    }

    private IEnumerator CooldownRoutine(string cooldownKey)
    {
        while (true)
        {
            float remaining = AbilityCooldownManager.Instance.GetRemainingCooldown(cooldownKey);
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
        topText.rectTransform.localScale = new Vector3(1f, 1f, 1f);
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
