using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Animator animator;

    public void SetText(string value)
    {
        text.text = value;
    }

    public void Show()
    {
        animator.Play("SpeechBubbleShow");
    }

    public void Hide()
    {
        animator.Play("SpeechBubbleHide");  
    }
}