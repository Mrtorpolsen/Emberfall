using UnityEngine;

public class SpeechBubbleComponent : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField] private SpeechBubble speechBubblePrefab;

    private SpeechBubble currentBubble;

    public void Speak(string text)
    {
        if (currentBubble == null)
        {
            currentBubble = Instantiate(speechBubblePrefab, anchor);

            float scale = anchor.lossyScale.x;
            currentBubble.transform.localScale /= scale;
        }

        currentBubble.SetText(text);
        currentBubble.Show();
    }

    public void StopSpeaking()
    {
        currentBubble?.Hide();
    }

}