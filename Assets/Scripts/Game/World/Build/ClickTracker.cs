using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickTracker : MonoBehaviour
{
    [SerializeField] private LayerMask clickableLayers;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Pointer.current == null) return;
        if (!Pointer.current.press.wasPressedThisFrame) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = cam.ScreenToWorldPoint(screenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, clickableLayers);

        if (!hit.collider) return;

        if (hit.collider.TryGetComponent(out BuildingPlot plot))
        {
            plot.OnPlotClicked();
        }
    }
}
