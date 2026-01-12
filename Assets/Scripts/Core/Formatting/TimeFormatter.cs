using UnityEngine;

public static class TimeFormatter
{
    public static string FormatTime(float timeInMilliseconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInMilliseconds / 1000f);
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
