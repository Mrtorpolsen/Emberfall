using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;

    public TMP_Text timerText;

    private float startTime;
    private bool timerRunning = false;
    private float updateTimer = 0f;


    public float GetElapsedTime() => (Time.time - startTime) * 1000f;
    public string GetFormattedTime() => TimeFormatter.FormatTime(GetElapsedTime());

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Update()
    {
        if (!timerRunning || timerText == null) return;

        updateTimer += Time.deltaTime;

        if (updateTimer >= 1f)
        {
            timerText.text = GetFormattedTime();
            updateTimer = 0f;
        }
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

}
