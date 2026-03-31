using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Slider slider;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDelay = 2f;
    [SerializeField] private float fadeDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    private Vector3 offset;
    private Transform targetTransform;
    private bool isActive;
    private float fadeTimer;
    private bool isFading;

    public bool IsActive => isActive;

    private void OnEnable()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        if (isFading)
        {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0f)
            {
                canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - Time.deltaTime / fadeDuration);
                if (canvasGroup.alpha <= 0f)
                {
                    isFading = false;
                    SetActive(false);
                }
            }
        }
    }

    public void Initialize(Transform target, Vector3 positionOffset)
    {
        this.targetTransform = target;
        this.offset = positionOffset;
        slider.value = 1f;
        SetActive(false);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth / maxHealth;
        
        // Check if healed to full health
        if (Mathf.Approximately(currentHealth, maxHealth) && isActive)
        {
            StartFadeTimer();
        }
    }

    private void StartFadeTimer()
    {
        isFading = true;
        fadeTimer = fadeDelay;
    }

    public void UpdatePosition(Vector3 worldPosition)
    {
        transform.position = worldPosition + offset;
        transform.rotation = Camera.main.transform.rotation;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        isFading = false;
        fadeTimer = 0f;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = active ? 1f : 0f;
        }
    }

    public Transform GetTarget() => targetTransform;

    public void Reset()
    {
        targetTransform = null;
        slider.value = 1f;
        isFading = false;
        fadeTimer = 0f;
        SetActive(false);
    }
}
