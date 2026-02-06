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
        overlay = root.Q<VisualElement>("LoadingOverlay");
        spinner = root.Q<VisualElement>("LoadingSpinner");
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
