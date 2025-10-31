using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            // Raycast to see what was clicked
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                // Check if the hit object has a BuildingPlot component
                BuildingPlot plot = hit.collider.GetComponent<BuildingPlot>();
                if (plot != null)
                {
                    plot.OnPlotClicked();
                }
            }
        }
    }
}
