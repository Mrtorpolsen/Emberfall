using UnityEngine;

public class SpawnMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private RectTransform menuPanel;



    public bool isOpen = false;

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            menuPanel.gameObject.SetActive(true);  // activate panel first
            panelAnimator.SetBool("isOpen", true);
        }
        else
        {
            panelAnimator.SetBool("isOpen", false);
            // Optional: disable after animation ends
        }
    }

}
