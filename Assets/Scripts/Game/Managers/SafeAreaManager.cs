using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class SafeAreaManager : MonoBehaviour
{
    public float targetVerticalSize = 5f; // Set this to your base vertical size (units)
    public float referenceAspect = 9f / 16f; // Your reference aspect ratio (width / height)

    void Update()
    {
        Camera cam = GetComponent<Camera>();
        float currentAspect = (float)Screen.width / Screen.height;

        // Keep vertical size constant
        cam.orthographicSize = targetVerticalSize;

        // Adjust viewport to handle wider/taller screens
        if (currentAspect > referenceAspect)
        {
            // Extra width — pillarbox
            float scaleWidth = referenceAspect / currentAspect;
            cam.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }
        else
        {
            // Extra height — letterbox
            float scaleHeight = currentAspect / referenceAspect;
            cam.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
    }
}
