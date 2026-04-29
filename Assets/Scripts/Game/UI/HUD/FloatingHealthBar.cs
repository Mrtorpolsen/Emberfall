using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float fadeDuration = 3f;

    public event Action<FloatingHealthBar> OnFadeComplete;
    private bool isFading;
    private Coroutine fadeRoutine;

    private Vector3 scale;

    public void Initialize(Vector3 scale)
    {
        this.scale = scale;

        transform.localScale = scale;

        isFading = false;
        fadeRoutine = null;
        canvasGroup.alpha = 1f;
    }

    public void SetPosition(Vector2 localPos)
    {
        rectTransform.localPosition = localPos;
    }

    public void UpdateValue(float current, float max)
    {
        slider.value = current / max;
    }

    public void TryStartFade()
    {
        if (isFading) return;

        isFading = true;
        fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;

        canvasGroup.alpha = 1f;

        while (t < fadeDuration)
        {
            if (!isFading) yield break;

            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        isFading = false;
        fadeRoutine = null;

        OnFadeComplete?.Invoke(this);
    }

    public void CancelFade()
    {
        if (!isFading)
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        isFading = false;

        canvasGroup.alpha = 1f;
    }
}