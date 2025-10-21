using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager main;

    public TMP_Text timerText;

    private float startTime;
    private bool timerRunning = false;

    private void Awake()
    {
        if(main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
    }

    public void StartTimer()
    {
        startTime = Time.time;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (!timerRunning) return;
        
        float elapsed = Time.time - startTime;
        int minutes = Mathf.FloorToInt(elapsed / 60);
        int seconds = Mathf.FloorToInt(elapsed % 60);

        if(timerText != null)
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public float GetElapsedTime() => Time.time - startTime;
}
