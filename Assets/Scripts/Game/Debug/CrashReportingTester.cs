using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;

// Debug-only trigger for checking Unity Diagnostics reporting on a device.
// Hold one finger in the top-left corner for holdSeconds: throws a managed exception.
// Hold one finger in the top-right corner for holdSeconds: hard native crash.
// Single-finger corner holds avoid OS multi-finger gestures (e.g. 3-finger screenshot).
// Does nothing outside development player builds, so it can stay on a scene object.
public class CrashReportingTester : MonoBehaviour
{
    [SerializeField] float holdSeconds = 3f;
    [SerializeField, Range(0.05f, 0.5f)] float cornerSize = 0.15f;

#if !DEVELOPMENT_BUILD || UNITY_EDITOR
    void Awake()
    {
        Destroy(this);
    }
#else
    enum Corner { None, TopLeft, TopRight }

    float holdTime;
    Corner heldCorner;

    void Awake()
    {
        Debug.Log("CrashReportingTester active: hold top-left for exception, top-right for crash");
    }

    void Update()
    {
        Corner corner = GetHeldCorner();

        // Restart the hold whenever the touch leaves or switches corner.
        if (corner != heldCorner)
        {
            heldCorner = corner;
            holdTime = 0f;
        }

        if (corner == Corner.None) return;

        holdTime += Time.unscaledDeltaTime;
        if (holdTime < holdSeconds) return;
        holdTime = 0f;

        if (corner == Corner.TopRight)
        {
            Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
        }
        else
        {
            throw new System.InvalidOperationException("Diagnostics test exception");
        }
    }

    Corner GetHeldCorner()
    {
        if (Touchscreen.current == null) return Corner.None;

        int fingers = 0;
        Vector2 position = default;
        foreach (var touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed) continue;
            fingers++;
            position = touch.position.ReadValue();
        }

        if (fingers != 1) return Corner.None;

        // Screen positions start at the bottom-left.
        float size = Mathf.Min(Screen.width, Screen.height) * cornerSize;
        if (position.y < Screen.height - size) return Corner.None;
        if (position.x <= size) return Corner.TopLeft;
        if (position.x >= Screen.width - size) return Corner.TopRight;
        return Corner.None;
    }
#endif
}
