using System.Collections;
using UnityEngine;

public static class Utility
{
    public static IEnumerator DoAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    public static string FormatTime(float timeInMilliseconds)
    {
        int totalSeconds = Mathf.FloorToInt(timeInMilliseconds / 1000f);
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}