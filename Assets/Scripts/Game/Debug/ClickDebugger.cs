using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickDebugger : MonoBehaviour
{
    public bool enabledDebug;

    void Update()
    {
        if (!enabledDebug) return;

        if (Touchscreen.current != null && Touchscreen.current.press.wasPressedThisFrame)
        {
            Vector2 pos = Touchscreen.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pos);
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log($"2D Hit: {hit.collider.name} | Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            }
            else
            {
                Debug.Log("2D Raycast hit NOTHING");
            }
        }
    }
}
