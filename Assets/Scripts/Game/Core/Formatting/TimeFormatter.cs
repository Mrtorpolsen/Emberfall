using UnityEngine;

public static class TimeFormatter
{
    public static string FormatTimeMiliseconds(float timeInMilliseconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInMilliseconds / 1000f);
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    public static string FormatTimeSeconds(float timeInseconds)
    {
        int minutes = Mathf.FloorToInt(timeInseconds / 60f);
        int seconds = Mathf.FloorToInt(timeInseconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
