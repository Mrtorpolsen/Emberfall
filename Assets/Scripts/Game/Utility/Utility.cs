using System.Collections;
using UnityEngine;

public static class Utility
{
    public static IEnumerator DoAfterDelay(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
}