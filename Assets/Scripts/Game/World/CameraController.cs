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

    private Vector2 previousTouchPosition; 
    private float previousPinchDistance;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
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
            HandlePinch(touches[0], touches[1]);
        }
    }

    private void HandlePan(Touch touch)
    {
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            previousTouchPosition = touch.screenPosition;
            return;
        }

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Moved)
            return;

        Vector3 previousWorld = ScreenToWorld(previousTouchPosition);
        Vector3 currentWorld = ScreenToWorld(touch.screenPosition);

        targetCamera.transform.position -= currentWorld - previousWorld;

        ClampCameraPosition();

        previousTouchPosition = touch.screenPosition;
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

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        targetCamera.transform.position = position;
    }

    private void Zoom(float zoomDelta, Vector2 screenPosition)
    {
        Vector3 worldBefore = ScreenToWorld(screenPosition);

        float newZoom = targetCamera.orthographicSize - zoomDelta;
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

        targetCamera.orthographicSize = newZoom;

        Vector3 worldAfter = ScreenToWorld(screenPosition);

        targetCamera.transform.position += worldBefore - worldAfter;

        ClampCameraPosition();
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
