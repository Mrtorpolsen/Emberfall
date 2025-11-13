using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class ResolutionManager : MonoBehaviour
{
    [SerializeField] private float targetAspect = 9f / 16f; // your base ratio
    [SerializeField] private float orthoSizeAtTarget = 5f;   // looks good on base ratio

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
#if UNITY_EDITOR
        UpdateCamera(); // live update in editor
#endif
    }

    void UpdateCamera()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        // If the current screen is taller, increase the orthographic size
        cam.orthographicSize = orthoSizeAtTarget * (targetAspect / currentAspect);
    }
}
