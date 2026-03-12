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

    public static string FormatDaysTime(float timeInSeconds)
    {
        int days = Mathf.FloorToInt(timeInSeconds / 86400f); // 24 * 60 * 60
        int hours = Mathf.FloorToInt((timeInSeconds % 86400f) / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        if (days > 0)
            return $"{days}d {hours:00}h {minutes:00}m {seconds:00}s";
        if (hours > 0)
            return $"{hours:00}h {minutes:00}m {seconds:00}s";
        return $"{minutes:00}m {seconds:00}s";
    }

    public static string FormatCondensedTime(float timeInSeconds)
    {
        int days = Mathf.FloorToInt(timeInSeconds / 86400f);
        int hours = Mathf.FloorToInt((timeInSeconds % 86400f) / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        if (days > 0)
            return $"{days:00}d {hours:00}h";
        if (hours > 0)
            return $"{hours:00}h {minutes:00}m";
        if (minutes > 0)
            return $"{minutes:00}m {seconds:00}s";

        return $"0m {seconds:00}s";
    }
}
