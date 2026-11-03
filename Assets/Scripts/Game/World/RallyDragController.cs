using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

// Drag the player's rally points with a finger (or the mouse in the Editor). Runs before CameraController and
// ClickTracker so they can see IsDragging on the frame a drag starts.
[DefaultExecutionOrder(-10)]
public class RallyDragController : MonoBehaviour
{
    [Tooltip("How close (world units) a press must be to a rally point to grab it")]
    [SerializeField] private float grabRadius = 0.6f;
    [Tooltip("Scale applied to a rally point while it's held")]
    [SerializeField] private float heldScale = 1.2f;

    public static bool IsDragging { get; private set; }

    private Camera cam;
    private Transform heldRally;
    private Vector3 heldRallyScale;
    private Vector2 grabOffset;
    private readonly List<RaycastResult> uiHits = new();

    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnDisable()
    {
        EndDrag();
    }

    private void Update()
    {
        if (Pointer.current == null)
            return;

        if (heldRally != null)
        {
            UpdateDrag();
            return;
        }

        if (Pointer.current.press.wasPressedThisFrame)
            TryBeginDrag();
    }

    private void TryBeginDrag()
    {
        if (PauseManager.IsPaused || GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        if (IsOverUI(screenPosition))
            return;

        Vector2 worldPosition = ScreenToWorld(screenPosition);
        Transform nearest = null;
        float nearestSqr = grabRadius * grabRadius;

        foreach (Transform rally in GameManager.Instance.DraggableRallies)
        {
            if (rally == null)
                continue;

            float sqr = ((Vector2)rally.position - worldPosition).sqrMagnitude;

            if (sqr <= nearestSqr)
            {
                nearest = rally;
                nearestSqr = sqr;
            }
        }

        if (nearest == null)
            return;

        heldRally = nearest;
        heldRallyScale = heldRally.localScale;
        heldRally.localScale = heldRallyScale * heldScale;
        // Keep the finger's offset so the rally doesn't jump under it
        grabOffset = (Vector2)heldRally.position - worldPosition;
        IsDragging = true;
    }

    private void UpdateDrag()
    {
        // Released, paused, game over, or a second finger (pinch zoom takes over)
        if (!Pointer.current.press.isPressed || PauseManager.IsPaused || GameManager.Instance.IsGameOver || HasMultipleTouches())
        {
            EndDrag();
            return;
        }

        Vector2 worldPosition = ScreenToWorld(Pointer.current.position.ReadValue());
        GameManager.Instance.MoveRally(heldRally, worldPosition + grabOffset);
    }

    private void EndDrag()
    {
        if (heldRally != null)
            heldRally.localScale = heldRallyScale;

        heldRally = null;
        IsDragging = false;
    }

    // Raycasts the UI directly: IsPointerOverGameObject can lag a frame behind touches on the press frame
    private bool IsOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = screenPosition };
        uiHits.Clear();
        EventSystem.current.RaycastAll(pointerData, uiHits);
        return uiHits.Count > 0;
    }

    private static bool HasMultipleTouches()
    {
        return EnhancedTouchSupport.enabled && Touch.activeTouches.Count > 1;
    }

    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return cam.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -cam.transform.position.z));
    }
}
