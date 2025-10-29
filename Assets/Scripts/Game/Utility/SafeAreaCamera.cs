using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class SafeAreaCamera : MonoBehaviour
{
    Camera cam;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplySafeArea();
    }

    void Update()
    {
        // Update in Editor or if orientation/resolution changes
        if (Application.isEditor || Screen.safeArea != lastSafeArea)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Convert safe area from absolute pixels (0 - Screen.width/height)
        // to normalized viewport coordinates (0 - 1)
        Rect normalized = new Rect(
            safeArea.x / Screen.width,
            safeArea.y / Screen.height,
            safeArea.width / Screen.width,
            safeArea.height / Screen.height
        );

        cam.rect = normalized;
    }
}
