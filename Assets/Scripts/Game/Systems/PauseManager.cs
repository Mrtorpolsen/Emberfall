using System;
using UnityEngine;


public static class PauseManager
{
    public static bool IsPaused { get; private set; } = false;
    public static event Action<bool> OnPauseChanged;

    public static void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public static void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        OnPauseChanged?.Invoke(paused);
    }
}
