using System;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Slider slider;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    public Vector3 Offset { get; private set; }

    private Vector3 scale;
    private Vector3 targetPosition;

    public void Initialize(Vector3 offset, Vector3 scale)
    {
        //Offset = offset;
        this.scale = scale;

        transform.localScale = scale;

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

}