using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager main;

    public TMP_Text timerText;

    private float startTime;
    private bool timerRunning = false;
    private float updateTimer = 0f;


    public float GetElapsedTime() => (Time.time - startTime) * 1000f;
    public string GetFormattedTime() => Utility.FormatTime(GetElapsedTime());

    private void Awake()
    {
        if(main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
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
