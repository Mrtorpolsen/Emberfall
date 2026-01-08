using UnityEngine;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Hit object: " + hit.collider.name);
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }
    }

}
