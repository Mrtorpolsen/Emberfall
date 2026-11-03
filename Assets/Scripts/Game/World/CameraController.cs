using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private BoxCollider2D cameraBounds;

    [Header("Zoom Values")]
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 7f;
    [SerializeField] private float zoomSpeed = 0.01f;

    [Header("Panning")]
    [Tooltip("If the view can move less than this (world units) on an axis, it stays centred on that axis instead of sliding a tiny bit")]
    [SerializeField] private float minPanRoom = 0.25f;
    [Tooltip("How far (dp, about 1/160 inch) a finger must move before the camera starts panning, so taps that wobble don't nudge it")]
    [SerializeField] private float panSlopDp = 8f;

    private Vector2 previousTouchPosition; 
    // The finger being tracked for panning, where it went down, and whether it has moved past the slop yet
    private int panTouchId = -1;
    private Vector2 panStartPosition;
    private bool isPanning;
    private float previousPinchDistance;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Start()
    {
        // The scene's starting size may show more than the bounds on this screen's aspect.
        targetCamera.orthographicSize = Mathf.Min(targetCamera.orthographicSize, GetMaxZoom());
        ClampCameraPosition();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        var touches = Touch.activeTouches;

        if (touches.Count == 1)
        {
            HandlePan(touches[0]);
        }
        else if (touches.Count == 2)
        {
            // The finger left after a pinch starts a fresh pan instead of jumping from its pre-pinch position
            panTouchId = -1;
            HandlePinch(touches[0], touches[1]);
        }
    }

    private void HandlePan(Touch touch)
    {
        // New finger, or the finger is moving a rally point: (re)start tracking from here, so panning doesn't
        // jump if a drag ends mid-touch
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || touch.touchId != panTouchId || RallyDragController.IsDragging)
        {
            panTouchId = touch.touchId;
            panStartPosition = touch.screenPosition;
            previousTouchPosition = touch.screenPosition;
            isPanning = false;
            return;
        }

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Moved)
            return;

        if (!isPanning)
        {
            if (Vector2.Distance(touch.screenPosition, panStartPosition) < GetPanSlopPixels())
                return;

            // Pan from where the finger went down, so the map stays under it instead of lagging the slop behind
            isPanning = true;
            previousTouchPosition = panStartPosition;
        }

        Vector3 previousWorld = ScreenToWorld(previousTouchPosition);
        Vector3 currentWorld = ScreenToWorld(touch.screenPosition);

        targetCamera.transform.position -= currentWorld - previousWorld;

        ClampCameraPosition();

        previousTouchPosition = touch.screenPosition;
    }

    private float GetPanSlopPixels()
    {
        // Screen.dpi is 0 when unknown (e.g. some editors); fall back to treating dp as pixels
        float dpi = Screen.dpi > 0f ? Screen.dpi : 160f;
        return panSlopDp * dpi / 160f;
    }

    private void HandlePinch(Touch firstTouch, Touch secondTouch)
    {
        Vector2 firstPosition = firstTouch.screenPosition;
        Vector2 secondPosition = secondTouch.screenPosition;

        Vector2 pinchCenter = (firstPosition + secondPosition) / 2f;
        float pinchDistance = Vector2.Distance(firstPosition, secondPosition); 
        // Establish the initial pinch distance.
        if (firstTouch.phase == UnityEngine.InputSystem.TouchPhase.Began 
            || secondTouch.phase == UnityEngine.InputSystem.TouchPhase.Began 
            || previousPinchDistance <= 0f) 
        { 
            previousPinchDistance = pinchDistance; 
            return; 
        } 

        float distanceDelta = pinchDistance - previousPinchDistance; 

        if (Mathf.Abs(distanceDelta) < 0.01f) 
            return; 

        Zoom(distanceDelta * zoomSpeed, pinchCenter);

        previousPinchDistance = pinchDistance;
    }

    private void ClampCameraPosition()
    {
        Bounds bounds = cameraBounds.bounds;

        float verticalExtent = targetCamera.orthographicSize;
        float horizontalExtent = verticalExtent * targetCamera.aspect;

        float minX = bounds.min.x + horizontalExtent;
        float maxX = bounds.max.x - horizontalExtent;

        float minY = bounds.min.y + verticalExtent;
        float maxY = bounds.max.y - verticalExtent;

        Vector3 position = targetCamera.transform.position;

        position.x = ClampAxis(position.x, minX, maxX, bounds.center.x);
        position.y = ClampAxis(position.y, minY, maxY, bounds.center.y);

        targetCamera.transform.position = position;
    }

    // On screens slightly off the bounds' aspect, max zoom leaves a sliver of room; panning through it just
    // jiggles the view (e.g. side to side on a missed rally tap), so that axis is locked to the centre
    private float ClampAxis(float value, float min, float max, float center)
    {
        if (max - min < minPanRoom)
            return center;

        return Mathf.Clamp(value, min, max);
    }

    private void Zoom(float zoomDelta, Vector2 screenPosition)
    {
        Vector3 worldBefore = ScreenToWorld(screenPosition);

        float newZoom = targetCamera.orthographicSize - zoomDelta;
        newZoom = Mathf.Clamp(newZoom, minZoom, GetMaxZoom());

        targetCamera.orthographicSize = newZoom;

        Vector3 worldAfter = ScreenToWorld(screenPosition);

        targetCamera.transform.position += worldBefore - worldAfter;

        ClampCameraPosition();
    }

    // Largest size whose view still fits inside the bounds, so fully zoomed out there's no room to pan on the tighter axis
    private float GetMaxZoom()
    {
        Bounds bounds = cameraBounds.bounds;
        float fitZoom = Mathf.Min(bounds.extents.y, bounds.extents.x / targetCamera.aspect);

        return Mathf.Max(minZoom, Mathf.Min(maxZoom, fitZoom));
    }

    private Vector3 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            -targetCamera.transform.position.z
        );

        return targetCamera.ScreenToWorldPoint(screenPoint);
    }
}
