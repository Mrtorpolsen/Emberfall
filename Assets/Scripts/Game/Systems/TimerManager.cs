using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public TMP_Text timerText;

    private bool timerRunning = false;
    private float updateTimer = 0f;
    private float elapsedTime = 0f;

    public float GetElapsedTimeInSeconds() => elapsedTime;
    public float GetElapsedTimeInMiliseconds() => elapsedTime * 1000f;
    public int GetElapsedTimeInMinutes() => Mathf.FloorToInt(elapsedTime / 60f);
    public string GetFormattedTime() => TimeFormatter.FormatTimeMiliseconds(GetElapsedTimeInMiliseconds());

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!timerRunning || timerText == null) return;

        elapsedTime += Time.deltaTime; // respects pause if Time.timeScale = 0
        updateTimer += Time.deltaTime;

        if (updateTimer >= 1f)
        {
            timerText.text = GetFormattedTime();
            updateTimer = 0f;
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        updateTimer = 0f;
    }

}
