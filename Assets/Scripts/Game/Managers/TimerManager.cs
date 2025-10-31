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
        
        if(timerText != null)
        {
            timerText.text = GetFormattedTime();
        }
    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public float GetElapsedTime() => Time.time - startTime;

    public string GetFormattedTime() => FormatTime(Time.time - startTime);

}
