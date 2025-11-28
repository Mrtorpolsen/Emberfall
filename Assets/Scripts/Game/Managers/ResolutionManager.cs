using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ResolutionManager : MonoBehaviour
{
    [SerializeField] private float targetAspect = 9f / 16f;
    [SerializeField] private float orthoSizeAtTarget = 5f;

    private Camera cam;
    private int lastW = 0, lastH = 0;

    private void OnEnable()
    {
        cam = GetComponent<Camera>();
        SafeUpdateCamera(true);
    }

    private void Update()
    {
        // --- Editor mode: update only when resolution actually changes ---
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (Screen.width != lastW || Screen.height != lastH)
            {
                SafeUpdateCamera();
            }
            return;
        }
#endif

        // --- Play mode: update once per frame (very cheap) ---
        SafeUpdateCamera();
    }

    /// <summary>
    /// Safely updates camera size without producing NaN/Infinity,
    /// even when the simulator or editor reports invalid screen sizes.
    /// </summary>
    private void SafeUpdateCamera(bool force = false)
    {
        int w = Screen.width;
        int h = Screen.height;

        // Prevent division by zero or invalid resolutions
        if (w <= 0 || h <= 0)
            return;

        // Skip if dimensions have not changed (unless forced)
        if (!force && w == lastW && h == lastH)
            return;

        lastW = w;
        lastH = h;

        float currentAspect = (float)w / h;

        // Extra sanity check
        if (currentAspect <= 0f)
            return;

        float newSize = orthoSizeAtTarget * (targetAspect / currentAspect);

        // Prevent NaN/Infinity issues
        if (float.IsNaN(newSize) || float.IsInfinity(newSize))
            return;

        cam.orthographicSize = 5f;
    }
}
