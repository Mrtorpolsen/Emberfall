using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;

    public TMP_Text timerText;

    private float startTime;
    private bool timerRunning = false;
    private float updateTimer = 0f;
    private float elapsedTime = 0f;


    public float GetElapsedTime() => elapsedTime * 1000f;
    public int GetElapsedTimeInMinutes() => Mathf.FloorToInt(elapsedTime / 60f);
    public string GetFormattedTime() => TimeFormatter.FormatTimeMiliseconds(GetElapsedTime());

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
