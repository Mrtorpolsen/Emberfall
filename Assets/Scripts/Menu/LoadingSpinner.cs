using UnityEngine;
using UnityEngine.UIElements;

public class LoadingSpinner : MonoBehaviour
{
    public static LoadingSpinner Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;

    VisualElement overlay;
    VisualElement spinner;
    IVisualElementScheduledItem spinTask;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var root = uiDocument.rootVisualElement;

        overlay = UtilityUIBinding.QRequired<VisualElement>(root, "LoadingOverlay");
        spinner = UtilityUIBinding.QRequired<VisualElement>(root, "LoadingSpinner");
    }

    public void ShowSpinner()
    {
        overlay.style.display = DisplayStyle.Flex;

        spinTask = spinner.schedule.Execute(() =>
        {
            var current = spinner.style.rotate.value.angle.value;
            spinner.style.rotate = new Rotate(new Angle(current + 6f, AngleUnit.Degree));
        }).Every(16);
    }

    public void HideSpinner()
    {
        spinTask?.Pause();
        overlay.style.display = DisplayStyle.None;
    }
}
